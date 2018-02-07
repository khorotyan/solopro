"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const express = require("express");
const morgan = require("morgan");
const bodyParser = require("body-parser");
const mongoose = require("mongoose");
const UserRouter_1 = require("./routes/UserRouter");
class App {
    constructor() {
        this.app = express();
        this.config();
        this.routes();
        this.routeChecks();
    }
    // Application config
    config() {
        require('dotenv').config();
        const conStr = 'mongodb://vahagn_khorotyan:' + process.env.MONGO_ATLAS_PW + '@solopro-shard-00-00-9edzx.mongodb.net:27017,solopro-shard-00-01-9edzx.mongodb.net:27017,solopro-shard-00-02-9edzx.mongodb.net:27017/test?ssl=true&replicaSet=solopro-shard-0&authSource=admin';
        mongoose.connect(conStr, err => {
            if (err) {
                console.log('Unable to connect to the server. Please start the server. Error:', err);
            }
        });
        mongoose.Promise = global.Promise;
        this.app.use(morgan('dev'));
        this.app.use(bodyParser.json());
        this.app.use(bodyParser.urlencoded({ extended: false }));
        this.app.use((req, res, next) => {
            res.header('Access-Control-Allow-Origin', '*');
            res.header('Access-Control-Allow-Headers', 'Origin, X-Requested-With, Content-Type, Accept, Authorization');
            if (req.method === 'OPTIONS') {
                res.header('Access-Control-Allow-Methods', 'GET, POST, PUT, PATCH, DELETE');
                return res.status(200).json({});
            }
            next();
        });
    }
    // Application routes
    routes() {
        const router = express.Router();
        this.app.use('/', router);
        this.app.use('/users', UserRouter_1.default);
    }
    routeChecks() {
        // Whenever different requests are sent, display some error message
        this.app.use((req, res, next) => {
            const reqError = new Error('Request Not Found');
            reqError.status = 404;
            next(reqError); // Forwards the error request
        });
        this.app.use((reqError, req, res, next) => {
            res.status(reqError.status || 500);
            res.json({
                message: reqError.message,
            });
        });
    }
}
exports.default = new App().app;
//# sourceMappingURL=app.js.map