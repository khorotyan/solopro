"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const mongoose_1 = require("mongoose");
const userSchema = new mongoose_1.Schema({
    _id: mongoose_1.Schema.Types.ObjectId,
    username: { type: String, required: true, unique: true },
    wins: { type: Number, default: 0 },
    email: { type: String, required: true, unique: true },
    password: { type: String, required: true },
    male: { type: Boolean, required: true },
    online: { type: Boolean, default: true }
});
exports.default = mongoose_1.model('User', userSchema);
//# sourceMappingURL=User.js.map