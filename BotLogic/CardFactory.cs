using System;
using System.Collections.Generic;
using DSharpPlus.Entities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using VFatumbot.BotLogic;
using static VFatumbot.BotLogic.Enums;

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

        public static IMessageActivity[] CreateLocationCardsReply(ChannelPlatform platform, double[] incoords, bool showStreetAndEarthThumbnails = false, dynamic w3wResult = null)
        {
            if (platform == ChannelPlatform.discord)
            {
                var embed = new DiscordEmbedBuilder();

                embed.Author = new DiscordEmbedBuilder.EmbedAuthor();
                embed.Author.Name = w3wResult?.words;
                embed.Color = DiscordColor.Azure;

                embed.Title = "View on Google Maps";
                embed.Url = CreateGoogleMapsUrl(incoords);

                embed.Description = $"Street View:\n{CreateGoogleStreetViewUrl(incoords)}\n\nGoogle Earth:\n{CreateGoogleEarthUrl(incoords)}";
                embed.ImageUrl = CreateGoogleMapsStaticThumbnail(incoords);
                embed.ThumbnailUrl = CreateGoogleStreetViewThumbnailUrl(incoords);

                embed.Build();

                var entity = new Entity();
                entity.SetAs(embed);

                var replyActivity = new Activity(entities: new List<Entity> {{ entity }}, type: "DiscordEmbed");

                return new IMessageActivity[] { replyActivity };
            }

            var useNativeLocationWidget = platform == ChannelPlatform.telegram || platform == ChannelPlatform.line;

            var replies = new IMessageActivity[useNativeLocationWidget ? 2 : 1];

            if (useNativeLocationWidget)
            {
                var nativeLocationWidgetReply = MessageFactory.Text("");
                var entity = new Entity();
                var geo = new GeoCoordinates(latitude: incoords[0],
                                             longitude: incoords[1],
                                             elevation: 0);
                var place = new Place(name: w3wResult != null ? "What 3 Words Address" : "Location",
                                       address: w3wResult != null ? w3wResult.words : "",
                                       geo: geo);
                entity.SetAs(place);
                nativeLocationWidgetReply.Entities = new List<Entity>() { entity };
                replies[1] = nativeLocationWidgetReply;
            }

            var attachments = new List<Attachment>();
            var attachmentReply = MessageFactory.Attachment(attachments);
            replies[0] = attachmentReply;

            attachmentReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            attachmentReply.Attachments.Add(CreateGoogleMapCard(incoords, !useNativeLocationWidget || showStreetAndEarthThumbnails, showStreetAndEarthThumbnails, w3wResult));

            if (showStreetAndEarthThumbnails)
            {
                attachmentReply.Attachments.Add(CreateGoogleStreetViewCard(incoords));
                attachmentReply.Attachments.Add(CreateGoogleEarthCard(incoords));
            }

            return replies;
        }

        public static Attachment CreateGoogleMapCard(double[] incoords, bool showMapsThumbnail, bool showStreetAndEarthThumbnails = false, dynamic w3wResult = null)
        {
            var images = new List<CardImage>();
            if (showMapsThumbnail)
            {
                images.Add(new CardImage(CreateGoogleMapsStaticThumbnail(incoords)));
            }

            var cardAction = new CardAction(ActionTypes.OpenUrl, showStreetAndEarthThumbnails ? "Open" : "Maps", value: CreateGoogleMapsUrl(incoords));

            var buttons = new List<CardAction> {
                cardAction,
            };

            if (!showStreetAndEarthThumbnails)
            {
                buttons.Add(new CardAction(ActionTypes.OpenUrl, "Street View", value: CreateGoogleStreetViewUrl(incoords)));
                buttons.Add(new CardAction(ActionTypes.OpenUrl, "Earth", value: CreateGoogleEarthUrl(incoords)));
            }

            var heroCard = new HeroCard
            {
                Title = !showStreetAndEarthThumbnails ? "View with Google:" : "Google Maps",
                Text = w3wResult?.words,
                Images = images,
                Buttons = buttons,
                Tap = cardAction
            };

            return heroCard.ToAttachment();
        }

        public static Attachment CreateGoogleStreetViewCard(double[] incoords)
        {
            var images = new List<CardImage> {
                new CardImage(CreateGoogleStreetViewThumbnailUrl(incoords)),
            };

            var cardAction = new CardAction(ActionTypes.OpenUrl, "Open", value: CreateGoogleStreetViewUrl(incoords));

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
                new CardImage(CreateGoogleEarthThumbnailUrl(incoords)),
            };

            var cardAction = new CardAction(ActionTypes.OpenUrl, "Open", value: CreateGoogleEarthUrl(incoords));

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

        public static string CreateGoogleMapsStaticThumbnail(double[] incoords)
        {
            return "https://maps.googleapis.com/maps/api/staticmap?&markers=color:red%7Clabel:C%7C" + incoords[0] + "+" + incoords[1] + "&zoom=15&size=" + Consts.THUMBNAIL_SIZE + "&maptype=roadmap&key=" + Consts.GOOGLE_MAPS_API_KEY;
        }

        public static string CreateGoogleMapsUrl(double[] incoords)
        {
            return "https://www.google.com/maps/place/" + incoords[0] + "+" + incoords[1] + "/@" + incoords[0] + "+" + incoords[1] + ",14z";
        }

        public static string CreateGoogleStreetViewUrl(double[] incoords)
        {
            return "https://www.google.com/maps/@?api=1&map_action=pano&viewpoint=" + incoords[0] + "," + incoords[1] + "&fov=90&heading=235&pitch=10";
        }

        public static string CreateGoogleEarthUrl(double[] incoords)
        {
            return "https://earth.google.com/web/search/" + incoords[0] + "," + incoords[1];
        }

        public static string CreateGoogleStreetViewThumbnailUrl(double[] incoords)
        {
            return "https://maps.googleapis.com/maps/api/streetview?size=" + Consts.THUMBNAIL_SIZE + "&location=" + incoords[0] + "," + incoords[1] + "&fov=90&heading=235&pitch=10&key=" + Consts.GOOGLE_MAPS_API_KEY;
        }

        public static string CreateGoogleEarthThumbnailUrl(double[] incoords)
        {
            return "https://maps.googleapis.com/maps/api/staticmap?&markers=color:red%7Clabel:C%7C" + incoords[0] + "+" + incoords[1] + "&zoom=18&size=" + Consts.THUMBNAIL_SIZE + "&maptype=satellite&key=" + Consts.GOOGLE_MAPS_API_KEY;
        }
    }
}
