const io = require('socket.io');
module.exports = function (server) {
    io(server).on('connection', (socket) => {
        console.log('a user connected');
        socket.on('disconnect', () => {
            console.log('user disconnected');
        });
    });
};
//# sourceMappingURL=challenge.js.map