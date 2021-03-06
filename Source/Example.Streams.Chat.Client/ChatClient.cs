﻿using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Typed;

using Orleans;
using Orleans.Streams;
using Orleans.Providers.Streams.SimpleMessageStream;

namespace Example
{
    class ChatClient
    {
        readonly TypedActorRef<ChatUser> user;
        readonly StreamRef room;

        StreamSubscriptionHandle<object> subscription;

        public ChatClient(IActorSystem system, string user, string room)
        {
            this.user = system.TypedActorOf<ChatUser>(user);
            this.room = system.StreamOf<SimpleMessageStreamProvider>(room);
        }

        public async Task Join()
        {
            subscription = await room.SubscribeAsync<ChatRoomMessage>((msg, token) =>
            {
                if (msg.User != UserName)
                    Console.WriteLine(msg.Text);

                return TaskDone.Done;
            });

            await user.Call(x => x.Join(RoomName));
        }

        public async Task Leave()
        {
            await subscription.UnsubscribeAsync();
            await user.Call(x => x.Leave(RoomName));
        }

        public async Task Say(string message)
        {
            await user.Call(x => x.Say(RoomName, message));
        }

        string UserName
        {
            get { return user.Path.Id; }
        }        
        
        string RoomName
        {
            get { return room.Path.Id; }
        }
    }
}
