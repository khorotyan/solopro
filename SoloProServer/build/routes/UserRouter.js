"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const express_1 = require("express");
const mongoose = require("mongoose");
const bcrypt = require("bcrypt");
const User_1 = require("../models/User");
const CheckAuth_1 = require("../middleware/CheckAuth");
class UserRouter {
    constructor() {
        this.router = express_1.Router();
        this.routes();
    }
    // Gets all the users sort based on online and then number of wins
    GetAllUsers(req, res, next) {
        User_1.default.find()
            .sort({ online: -1, wins: -1 })
            .limit(100)
            .exec()
            .then((users) => {
            const response = {
                users: users.map((user) => ({
                    username: user.username,
                    wins: user.wins,
                    male: user.male,
                    online: user.online,
                })),
            };
            res.status(200).json(response);
        })
            .catch((err) => {
            res.status(500).json({ error: err });
        });
    }
    // Creates a new user
    CreateUser(req, res, next) {
        const username = req.body.username;
        const email = req.body.email;
        const male = req.body.male;
        /*
            bycrypt.hash will hash the password, however it would have still been vulnerable
            if the user typed a very easy password, e.g. monkey, then the dehash of it,
            could be easily found as every string has a unique hash version of it,
            for that reason, the second parameter of the function (salt) adds random
            strings to the password before hashing it, therefore making it more secure
        */
        User_1.default.findOne({ username: username })
            .exec()
            .then((doc) => {
            if (doc) {
                return res.status(422).json({ message: 'Username already exists' });
            }
            else {
                bcrypt.hash(req.body.password, 10, (err, hash) => {
                    if (err) {
                        return res.status(500).json({
                            error: err,
                        });
                        // If the creation of the hash is successful, only then create a new user
                    }
                    else {
                        const user = new User_1.default({
                            _id: new mongoose.Types.ObjectId(),
                            username: username,
                            email: email,
                            password: hash,
                            male: male,
                            online: true,
                        });
                        user.save()
                            .then((result) => {
                            const token = CheckAuth_1.default.GenerateToken(email, user._id);
                            return res.status(200).json({ token: token });
                        })
                            .catch((error) => {
                            res.status(500).json({ error: error });
                        });
                    }
                });
            }
        });
    }
    // Login a user
    UserLogin(req, res, next) {
        const email = req.body.email;
        const autologin = req.body.autologin;
        User_1.default.findOneAndUpdate({ email: email }, { online: true })
            .exec()
            .then((user) => {
            if (user) {
                if (autologin === false) {
                    // Compare the given password with the one in the databse which is hashed
                    bcrypt.compare(req.body.password, user.password, (err, result) => {
                        if (result) {
                            const token = CheckAuth_1.default.GenerateToken(email, user._id);
                            return res.status(200).json({
                                token: token,
                                username: user.username,
                                wins: user.wins,
                                male: user.male
                            });
                        }
                        res.status(401).json({ message: 'Authentication failed' });
                    });
                }
                else {
                    const token = CheckAuth_1.default.GenerateToken(email, user._id);
                    return res.status(200).json({
                        token: token,
                        username: user.username,
                        wins: user.wins,
                        male: user.male
                    });
                }
            }
            else {
                return res.status(401).json({ message: 'Authentication failed' });
            }
        })
            .catch((err) => {
            res.status(500).json({ error: err });
        });
    }
    // Set user login status
    UserLogout(req, res, next) {
        const email = req.body.email;
        User_1.default.findOneAndUpdate({ email: email }, { online: false })
            .exec()
            .then((result) => {
            res.status(200).json({ message: 'User logged off' });
        })
            .catch((err) => {
            res.status(500).json({ error: err });
        });
    }
    // Update number of won games of a player
    UpdateWins(req, res, next) {
        const email = req.body.email;
        const wins = req.body.wins;
        User_1.default.findOneAndUpdate({ email: email }, { wins: wins })
            .exec()
            .then((result) => {
            res.status(200).json({ message: 'User score updated' });
        })
            .catch((err) => {
            res.status(500).json({ error: err });
        });
    }
    // Get an individual user's information
    GetPlayer(req, res, next) {
        const email = req.params.email;
        User_1.default.findOne({ email: email })
            .exec()
            .then((doc) => {
            if (doc) {
                res.status(200).json({ doc });
            }
            else {
                res.status(404).json({ message: 'No user found with the provided email' });
            }
        }).catch((err) => {
            res.status(500).json({ error: err });
        });
    }
    // Setup the routes
    routes() {
        this.router.get('/', CheckAuth_1.default.LoginCheck, this.GetAllUsers);
        this.router.post('/signup', this.CreateUser);
        this.router.post('/login', this.UserLogin);
        this.router.post('/wins', CheckAuth_1.default.LoginCheck, this.UpdateWins);
        this.router.get('/:email', CheckAuth_1.default.LoginCheck, this.GetPlayer);
        this.router.post('/logout', CheckAuth_1.default.InvalidateToken, this.UserLogout);
    }
}
exports.default = new UserRouter().router;
//# sourceMappingURL=UserRouter.js.map