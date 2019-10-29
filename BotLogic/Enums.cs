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
            Pair,
            ScanAttractor,
            ScanVoid,
            ScanAnomaly,
            ScanPair,
            Quantum,
            QuantumTime,
            Pseudo,
            MysteryPoint
        }

        public enum TripRating
        {
            A = 1000,
            B = 2000,
            C = 3000,
            D = 4000,
            E = 5000,
            F = 6000,
        }
    }
}
