import { model, Schema } from 'mongoose';

const userSchema: Schema = new Schema({
    _id: Schema.Types.ObjectId,
    username: { type: String, required: true, unique: true },
    wins: { type: Number, default: 0 },
    email: { type: String, required: true, unique: true },
    password: { type: String, required: true},
    male: { type: Boolean, required: true },
    online: { type: Boolean, default: true },
});

export default model('User', userSchema);
