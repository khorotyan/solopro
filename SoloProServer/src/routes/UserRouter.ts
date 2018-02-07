import { Request, Response, Router, NextFunction } from 'express';
import * as mongoose from 'mongoose';
import * as bcrypt from 'bcrypt';
import * as jwt from 'jsonwebtoken';

import User from '../models/User';
import InvalidToken from '../models/InvalidToken';
import CheckAuth from '../middleware/CheckAuth';

class UserRouter {

    public router: Router;

    constructor() {
        this.router = Router();
        this.routes();
    }

    // Gets all the users sort based on online and then number of wins
    public GetAllUsers(req: Request, res: Response, next: NextFunction): void {

        User.find()
            .sort({online: -1, wins: -1})
            .limit(100)
            .exec()
            .then((users) => {
                const response = {
                    users: users.map((user) => ({
                        username: (user as any).username,
                        wins: (user as any).wins,
                        male: (user as any).male,
                        online: (user as any).online,
                    })),
                };

                res.status(200).json(response);
            })
            .catch((err) => {
                res.status(500).json({error: err});
            });
    }

    // Creates a new user
    public CreateUser(req: Request, res: Response, next: NextFunction): void {

        const username: string = req.body.username;
        const email: string = req.body.email;
        const male: boolean = req.body.male;

        /*
            bycrypt.hash will hash the password, however it would have still been vulnerable
            if the user typed a very easy password, e.g. monkey, then the dehash of it,
            could be easily found as every string has a unique hash version of it, 
            for that reason, the second parameter of the function (salt) adds random 
            strings to the password before hashing it, therefore making it more secure
        */
        User.findOne({username: username})
            .exec()
            .then((doc) => {
                if (doc) {
                    return res.status(422).json({message: 'Username already exists'});
                } else {
                    bcrypt.hash(req.body.password, 10, (err, hash) => {
                        if (err) {
                            return res.status(500).json({
                                error: err,
                            });
                        // If the creation of the hash is successful, only then create a new user
                        } else {
                            const user = new User({
                                _id: new mongoose.Types.ObjectId(),
                                username: username,
                                email: email,
                                password: hash,
                                male: male,
                                online: true,
                            });

                            user.save()
                                .then((result) => {
                                    const token: string = CheckAuth.GenerateToken(email, (user as any)._id);

                                    return res.status(200).json({token: token});
                                })
                                .catch((error) => {
                                    res.status(500).json({error: error});
                                });
                        }
                    });
                }
            });
    }

    // Login a user
    public UserLogin(req: Request, res: Response, next: NextFunction): void {

        const email: string = req.body.email;
        const autologin: boolean = req.body.autologin;

        User.findOneAndUpdate({email: email}, {online: true})
            .exec()
            .then((user) => {
                if (user) {

                    
                    
                    if (autologin === false) {
                        // Compare the given password with the one in the databse which is hashed
                        bcrypt.compare(req.body.password, (user as any).password, (err, result) => {
                            
                            if (result) {
                                const token: string = CheckAuth.GenerateToken(email, (user as any)._id);
                                
                                return res.status(200).json({
                                    token: token,
                                    username: (user as any).username,
                                    wins: (user as any).wins,
                                    male: (user as any).male
                                });
                            }
                            
                            res.status(401).json({message: 'Authentication failed'});
                        });
                    } else {
                        const token: string = CheckAuth.GenerateToken(email, (user as any)._id);

                        return res.status(200).json({
                            token: token,
                            username: (user as any).username,
                            wins: (user as any).wins,
                            male: (user as any).male
                        });
                    }
                } else {
                    return res.status(401).json({message: 'Authentication failed'});
                }
            })
            .catch((err) => {
                res.status(500).json({error: err});
            });
    }

    // Set user login status
    public UserLogout(req: Request, res: Response, next: NextFunction): void {
        
        const email: string = req.body.email;

        User.findOneAndUpdate({email: email}, {online: false})
            .exec()
            .then((result) => {
                res.status(200).json({message: 'User logged off'});
            })
            .catch((err) => {
                res.status(500).json({error: err});
            });
    }

    // Update number of won games of a player
    public UpdateWins(req: Request, res: Response, next: NextFunction): void { 
        
        const email: string = req.body.email;
        const wins: number = req.body.wins;

        User.findOneAndUpdate({email: email}, {wins: wins})
            .exec()
            .then((result) => {
                res.status(200).json({message: 'User score updated'});
            })
            .catch((err) => {
                res.status(500).json({error: err});
            });
    }

    // Get an individual user's information
    public GetPlayer(req: Request, res: Response, next: NextFunction): void {

        const email: string = req.params.email;

        User.findOne({email: email})
            .exec()
            .then((doc) => {
                if (doc) {
                    res.status(200).json({doc});
                } else {
                    res.status(404).json({message: 'No user found with the provided email'})
                }
            }).catch((err) => {
                res.status(500).json({error: err});
            });
    }

    // Setup the routes
    public routes() {
        this.router.get('/', CheckAuth.LoginCheck, this.GetAllUsers);
        this.router.post('/signup', this.CreateUser);
        this.router.post('/login', this.UserLogin);
        this.router.post('/wins', CheckAuth.LoginCheck, this.UpdateWins);
        this.router.get('/:email', CheckAuth.LoginCheck, this.GetPlayer);
        this.router.post('/logout', CheckAuth.InvalidateToken, this.UserLogout);
    }
}

export default new UserRouter().router;
