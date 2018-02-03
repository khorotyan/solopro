import { Request, Response, Router, NextFunction } from 'express';
import * as mongoose from 'mongoose';
import * as jwt from 'jsonwebtoken';

import InvalidToken from '../models/InvalidToken';

class CheckAuth {

    // Check if the user is logged in
    public LoginCheck(req: Request, res: Response, next: NextFunction): Response {
        try {          
            const token = (<string>req.headers.authorization).split(" ")[1];
            // If the token does not exist in the blacklist token array, then only login
            InvalidToken.findOne({token: token})
                .exec()
                .then(result => {
                    if (result) {
                        return res.status(401).json({message: 'Authentication failed'});
                    } else {
                        const decoded = jwt.verify(token, process.env.JWT_KEY);
                        (<any>req).userData = decoded;
                        next();
                    }
                }).catch(err => {
                    res.status(500).json({error: err});
                }); 
        } catch (err) {
            return res.status(401).json({message: 'Authentication failed'});
        }
    }

    // Generate a token at signin which we use at sending other queries within the app
    public GenerateToken = (email: string, userID: string): string => {
          
        const token: string = jwt.sign({
                email: email,
                userID: userID
            }, 
            process.env.JWT_KEY, 
            {
                // Our token will expire in 2h thus login will be required again
                expiresIn: '2h'
            });

        return token;
    }   

    // Logout a user by invalidating the current token, thus no one
    //  will be able to log into the current user's account without generating a new token
    public InvalidateToken(req: Request, res: Response, next: NextFunction): void {
        try {
            const token: string = (<string>req.headers.authorization).split(" ")[1];
        
            const invalidToken = new InvalidToken({
                _id: new mongoose.Types.ObjectId(),
                token: token
            });
            
            invalidToken.save()
                .catch(err => {
                    res.status(500).json({error: err});
                });
        } catch (err) {
            res.status(500).json({error: err});
        } 
        next();
    }
}

export default new CheckAuth();
