"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const mongoose_1 = require("mongoose");
// Collect the invalid tokens to prevent user login after the 
//  user logs out, for that reason autodestroy the token blacklist 
//  after 2 hours
const invalidTokenSchema = new mongoose_1.Schema({
    _id: mongoose_1.Schema.Types.ObjectId,
    token: { type: String, expires: 2 * 60 * 60 }
});
exports.default = mongoose_1.model('InvalidToken', invalidTokenSchema);
//# sourceMappingURL=InvalidToken.js.map