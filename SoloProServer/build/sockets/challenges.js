const io = require('socket.io');
module.exports = server => {
    const cnsp = io(server).of('/challenges');
    cnsp.on('connection', (socket) => {
        // When a user challenges another one, change the player and opponent fields and 
        //  send the data back to the users
        socket.on('CHALLENGE_SENT', (data) => {
            data = JSON.parse(data);
            const challenge = {
                player: data.opponent,
                opponent: data.player
            };
            socket.broadcast.emit('CHALLENGE_SENT_BACK', challenge);
        });
        socket.on('CHALLENGE_ACCREJ', (data) => {
            data = JSON.parse(data);
            const challenge = {
                player: data.opponent,
                opponent: data.player,
                accepted: data.accepted
            };
            socket.broadcast.emit('CHALLENGE_ACCREJ_BACK', challenge);
        });
        socket.on('ANSWER_SENT', (data) => {
            data = JSON.parse(data);
            const info = {
                player: data.opponent,
                opponent: data.player,
                points: data.points
            };
            socket.broadcast.emit('ANSWER_SENT_BACK', info);
        });
        socket.on('disconnect', () => {
            //socket.broadcast.emit("USER_DISCONNECT_BACK", challenge);
            console.log('User disconnected');
        });
    });
};
//# sourceMappingURL=challenges.js.map