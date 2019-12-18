using System;
namespace VFatumbot.BotLogic
{
    public class Enums
    {
        public enum ChannelPlatform
        {
            /** IMPORTANT: Don't change order/delete as their corresponding ints are saved in the DB
              * To add new ones, just add to the bottom of the list.
              * Most of these will relate to context.Activity.ChannelId
              */
            emulator,
            directline,
            facebook,
            telegram,
            line,
            discord,
            slack,
            skype
        }

        public enum WebSrc
        {
            nonweb,
            web,
            android,
            ios
        }

        public enum PointTypes
        {
            /** IMPORTANT: Don't change order/delete as their corresponding ints are saved in the DB
              * To add new ones, just add to the bottom of the list.
              * Most of these will relate to context.Activity.ChannelId
              */
            Attractor,
            Void,
            Anomaly,
            PairAttractor,
            PairVoid,
            ScanAttractor,
            ScanVoid,
            ScanAnomaly,
            ScanPair,
            Quantum,
            QuantumTime,
            Pseudo,
            MysteryPoint,
            ChainAttractor,
            ChainVoid,
            ChainAnomaly,
            ChainQuantum,
            ChainPseudo,
            //ChainMysteryPoint // not implemented yet
        }
    }
}
