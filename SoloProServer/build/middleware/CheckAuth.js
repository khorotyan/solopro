"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const mongoose = require("mongoose");
const jwt = require("jsonwebtoken");
const InvalidToken_1 = require("../models/InvalidToken");
class CheckAuth {
    constructor() {
        // Generate a token at signin which we use at sending other queries within the app
        this.GenerateToken = (email, userID) => {
            const token = jwt.sign({
                email: email,
                userID: userID
            }, process.env.JWT_KEY, {
                // Our token will expire in 2h thus login will be required again
                expiresIn: '2h'
            });
            return token;
        };
    }
    // Check if the user is logged in
    LoginCheck(req, res, next) {
        try {
            const token = req.headers.authorization.split(" ")[1];
            // If the token does not exist in the blacklist token array, then only login
            InvalidToken_1.default.findOne({ token: token })
                .exec()
                .then(result => {
                if (result) {
                    return res.status(401).json({ message: 'Authentication failed' });
                }
                else {
                    const decoded = jwt.verify(token, process.env.JWT_KEY);
                    req.userData = decoded;
                    next();
                }
            }).catch(err => {
                res.status(500).json({ error: err });
            });
        }
        catch (err) {
            return res.status(401).json({ message: 'Authentication failed' });
        }
    }
    // Logout a user by invalidating the current token, thus no one
    //  will be able to log into the current user's account without generating a new token
    InvalidateToken(req, res, next) {
        try {
            const token = req.headers.authorization.split(" ")[1];
            const invalidToken = new InvalidToken_1.default({
                _id: new mongoose.Types.ObjectId(),
                token: token
            });
            invalidToken.save()
                .catch(err => {
                res.status(500).json({ error: err });
            });
        }
        catch (err) {
            res.status(500).json({ error: err });
        }
        next();
    }
}
exports.default = new CheckAuth();
//# sourceMappingURL=CheckAuth.js.map