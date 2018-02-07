"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const io = require("socket.io");
module.exports = (server) => {
    const cnsp = io(server).of('/challenges');
    cnsp.on('connection', (socket) => {
        // When a user challenges another one, change the player and opponent fields and 
        //  send the data back to the users
        socket.on('CHALLENGE_SENT', (data) => {
            data = JSON.parse(data);
            const challenge = {
                player: data.opponent,
                opponent: data.player,
                male: data.male
            };
            socket.broadcast.emit('CHALLENGE_SENT_BACK', challenge);
        });
        socket.on('CHALLENGE_ACCREJ', (data) => {
            data = JSON.parse(data);
            const challenge = {
                player: data.opponent,
                opponent: data.player,
                accepted: data.accepted,
                questions: data.questions,
                cancelled: data.cancelled
            };
            socket.broadcast.emit('CHALLENGE_ACCREJ_BACK', challenge);
        });
        socket.on('ANSWER_SENT', (data) => {
            data = JSON.parse(data);
            const challenge = {
                player: data.opponent,
                opponent: data.player,
                points: data.points
            };
            socket.broadcast.emit('ANSWER_SENT_BACK', challenge);
        });
    });
};
//# sourceMappingURL=challenges.js.map