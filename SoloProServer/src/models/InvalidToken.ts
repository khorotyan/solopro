import { model, Schema } from 'mongoose';

// Collect the invalid tokens to prevent user login after the 
//  user logs out, for that reason autodestroy the token blacklist 
//  after 2 hours

const invalidTokenSchema: Schema = new Schema({
    _id: Schema.Types.ObjectId,
    token: {type: String, expires: 2 * 60 * 60},
});

export default model('InvalidToken', invalidTokenSchema);
