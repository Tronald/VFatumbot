using System;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.Bot.Builder;
using VFatumbot.BotLogic;

namespace VFatumbot
{
    public class QuantumRandomNumberGeneratorWrapper
    {
        protected readonly ITurnContext _turnContext;
        protected readonly MainDialog _mainDialog;
        protected readonly CancellationToken _cancellationToken;
        protected readonly QuantumRandomNumberGenerator qRNG;

        public class CanIgnoreException : Exception { }

        public QuantumRandomNumberGeneratorWrapper(ITurnContext turnContext, MainDialog mainDialog, CancellationToken cancellationToken)
        {
            _turnContext = turnContext;
            _mainDialog = mainDialog;
            _cancellationToken = cancellationToken;
            qRNG = new QuantumRandomNumberGenerator();
        }

        public int Next(int maxValue)
        {
            try
            {
                return qRNG.Next(maxValue);
            }
            catch (Exception e)
            {
                if (!HandleException(e))
                {
                    throw e;
                }

                throw new CanIgnoreException();
            }
        }

        public int Next(int minValue, int maxValue)
        {
            try
            {
                return qRNG.Next(minValue, maxValue);
            }
            catch (Exception e)
            {
                if (!HandleException(e))
                {
                    throw e;
                }

                throw new CanIgnoreException();
            }
        }

        public string NextHex(int len)
        {
            try
            {
                return qRNG.NextHex(len);
            }
            catch (Exception e)
            {
                if (!HandleException(e))
                {
                    throw e;
                }

                throw new CanIgnoreException();
            }
        }

        public byte[] NextHexBytes(int len, int meta)
        {
            try
            {
                return qRNG.NextHexBytes(len, meta);
            }
            catch (Exception e)
            {
                if (!HandleException(e))
                {
                    throw e;
                }

                throw new CanIgnoreException();
            }
        }

        private bool HandleException(Exception exception)
        {
            // Here's a dirty hack (the codebase is starting to get filled with lots of these :-))
            // to catch QRNG source related exceptions to allow us to do operations like
            // send the user a message, reset their scanning flags and take them back to MainDialog prompt
            if ((exception.GetType().Equals(typeof(InvalidDataException)) && "Service did not return random data.".Equals(exception.Message)) ||
                    (exception.GetType().Equals(typeof(WebException)) && exception.Message.Contains("connection attempt failed because the connected party did not properly respond after a period of time")))
            {
                _turnContext.SendActivityAsync("Sorry, there was an error sourcing quantum entropy needed to randomize. Try a bit later. If this happens during beta testing tell soliax.").GetAwaiter().GetResult();
                ((AdapterWithErrorHandler)_turnContext.Adapter).RepromptMainDialog(_turnContext, _mainDialog, _cancellationToken, new CallbackOptions() { ResetFlag = true }).GetAwaiter().GetResult();
                return true;
            }

            return false;
        }
    }
}