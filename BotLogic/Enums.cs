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
            webchat,
            facebook,
            telegram,
            line,
        }

        public enum TripRating
        {
            /** IMPORTANT: Don't change order/delete as their corresponding ints are saved in the DB
              * To add new ones, just add to the bottom of the list.
              */
            Life_changing,
            Very_meaningful,
            Meaningful,
            Non_meaningful,
            Pleasant,
            Plain,
            Waste_of_time,
            Unpleasant,
            Very_unpleasant,
        }
    }
}
