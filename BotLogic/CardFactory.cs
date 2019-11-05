using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using VFatumbot.BotLogic;

namespace VFatumbot
{
    public static class CardFactory
    {
        public static IMessageActivity CreateGetLocationFromGoogleMapsReply()
        {
            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);

            var cardAction = new CardAction(ActionTypes.OpenUrl, "Open Google Maps App", value: "https://maps.google.com");

            var buttons = new List<CardAction> {
                cardAction
            };

            var heroCard = new HeroCard
            {
                Title = "Facebook's removing the Send Location button",
                Text = "Maps→Longpress & drop pin→Share to Messenger→Randonauts→Back to chat",
                Tap = cardAction,
                Buttons = buttons,
            };

            reply.Attachments.Add(heroCard.ToAttachment());

            return reply;
        }

        public static IMessageActivity CreateLocationCardsReply(double[] incoords, dynamic w3wResult = null)
        {
            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments.Add(CreateGoogleMapCard(incoords, w3wResult));
            //reply.Attachments.Add(CreateGoogleStreetViewCard(incoords));
            //reply.Attachments.Add(CreateGoogleEarthCard(incoords));
            return reply;
        }

        public static Attachment CreateGoogleMapCard(double[] incoords, dynamic w3wResult = null)
        {
            var images = new List<CardImage> {
                new CardImage("https://maps.googleapis.com/maps/api/staticmap?&markers=color:red%7Clabel:C%7C" + incoords[0] + "+" + incoords[1] + "&zoom=15&size=" + Consts.THUMBNAIL_SIZE + "&maptype=roadmap&key=" + Consts.GOOGLE_MAPS_API_KEY),
            };

            var cardAction = new CardAction(ActionTypes.OpenUrl, "Maps", value: "https://www.google.com/maps/place/" + incoords[0] + "+" + incoords[1] + "/@" + incoords[0] + "+" + incoords[1] + ",18z");

            var buttons = new List<CardAction> {
                cardAction,
                new CardAction(ActionTypes.OpenUrl, "Street View", value: "https://www.google.com/maps/@?api=1&map_action=pano&viewpoint=" + incoords[0] + "," + incoords[1] + "&fov=90&heading=235&pitch=10"),
                new CardAction(ActionTypes.OpenUrl, "Earth", value: "https://earth.google.com/web/@" + incoords[0] + "," + incoords[1] + ",146.726a,666.616d,35y,0h,45t,0r")
            };

            var heroCard = new HeroCard
            {
                //Title = "Google Map",
                //Text = w3wResult != null ? ("What 3 Words address: " + w3wResult.words) : null,
                Title = w3wResult != null ? ("W3W: " + w3wResult.words) : null,
                Images = images,
                Buttons = buttons,
                Tap = cardAction
            };

            return heroCard.ToAttachment();
        }

        public static Attachment CreateGoogleStreetViewCard(double[] incoords)
        {
            var images = new List<CardImage> {
                new CardImage("https://maps.googleapis.com/maps/api/streetview?size=" + Consts.THUMBNAIL_SIZE + "&location=" + incoords[0] + "," + incoords[1] + "&fov=90&heading=235&pitch=10&key=" + Consts.GOOGLE_MAPS_API_KEY),
            };

            var cardAction = new CardAction(ActionTypes.OpenUrl, "Open", value: "https://www.google.com/maps/@?api=1&map_action=pano&viewpoint=" + incoords[0] + "," + incoords[1] + "&fov=90&heading=235&pitch=10");

            var buttons = new List<CardAction> {
                cardAction
            };

            var heroCard = new HeroCard
            {
                Title = "Google Street View",
                Images = images,
                Buttons = buttons,
                Tap = cardAction,
            };

            return heroCard.ToAttachment();
        }

        public static Attachment CreateGoogleEarthCard(double[] incoords)
        {
            var images = new List<CardImage> {
                new CardImage("https://maps.googleapis.com/maps/api/staticmap?&markers=color:red%7Clabel:C%7C" + incoords[0] + "+" + incoords[1] + "&zoom=18&size=" + Consts.THUMBNAIL_SIZE + "&maptype=satellite&key=" + Consts.GOOGLE_MAPS_API_KEY),
            };

            var cardAction = new CardAction(ActionTypes.OpenUrl, "Open", value: "https://earth.google.com/web/@" + incoords[0] + "," + incoords[1] + ",146.726a,666.616d,35y,0h,45t,0r");

            var buttons = new List<CardAction> {
                cardAction
            };

            var heroCard = new HeroCard
            {
                Title = "Google Earth View",
                Images = images,
                Buttons = buttons,
                Tap = cardAction
            };

            return heroCard.ToAttachment();
        }
    }
}
