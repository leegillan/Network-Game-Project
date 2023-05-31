using System;
using System.Collections.Generic;
using System.Text;

//Package installed to allow for vectors to be used through server
using System.Numerics;

namespace CMP303Game
{
    //Hosts the player data and logic of the game
    class Player
    { 
        public int id;
        public string username;

        public Vector3 position;
        public Quaternion rotation;

        //Acts as dividing by delta time
        private float moveSpeed = 5.0f / Constants.TICKS_PER_SEC;

        private bool[] inputs;

        public Player(int _id, string _username, Vector3 _SpawnPosition)
        {
            id = _id;
            username = _username;
            position = _SpawnPosition;
            rotation = Quaternion.Identity;

            inputs = new bool[4];
        }

        public void Update()
        {
            Vector2 inputDirection =  Vector2.Zero;

            if(inputs[0])
            {
                inputDirection.Y += 1;
            }
            if (inputs[1])
            {
                inputDirection.Y -= 1;
            }
            if (inputs[2])
            {
                inputDirection.X += 1;
            }
            if (inputs[3])
            {
                inputDirection.X -= 1;
            }

            Move(inputDirection);
        }

        private void Move(Vector2 inputDirection)
        {
            //Players direction
            //The way the player is facing
            Vector3 forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);

            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, new Vector3(0, 1, 0)));

            Vector3 moveDirection = right * inputDirection.X + forward * inputDirection.Y;

            position += moveDirection * moveSpeed;

            //Send data
            ServerSend.PlayerPosition(this);

            //Send rotation to everyone except who it belongs to so that the server 
            //does not overwrite the player rotation and cause any snapping
            ServerSend.PlayerRotation(this);
        }

        public void SetInput(bool[] _input, Quaternion _rot)
        {
            inputs = _input;
            rotation = _rot;
        }
    }
}
