# README #

SoloPro is a multiplayer trivia application. It was given as a task from SoloLearn Inc for a backend position. The backend is done with node.js. Matchmaking, lobby, and the quiz are done with sockets using the well known socket.io library.

Watch a video showing how to run the program and at the same time showing the overall functionality of the application:
https://drive.google.com/file/d/1HSskH8_k3xTyIEEKS8uq-ta2Bc-R2mWb/view

Some of the application features are:
1. Registration / Login (you cannot login to your account from multiple devices at the same time)
2. Challenges panel showing online users and number of wins they have
3. A challenge can be sent to the online users
4. Challenge notification is almost instantly transfered due to sockets
5. Upon approval, both players enter to the lobby
6. If both of the players stay in the lobby during the required time, the game starts, else cancels
7. Question numbers are shared with sockets
8. When a player answers a question, the score is instantly updated on the other user device (sockets)
9. After the end of the game the number of win fields is updated
10. if a player leaves during the game, it continues for the other player,
if instead it was canceled, it could have make a problem as all the players who were about to lose would leave the game in order not to lose 
