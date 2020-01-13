using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.CSharp.RuntimeBinder;
using VFatumbot.BotLogic;

namespace VFatumbot
{
    public class QuantumRandomNumberGeneratorWrapper
    {
        protected readonly ITurnContext _turnContext;
        protected readonly MainDialog _mainDialog;
        protected readonly CancellationToken _cancellationToken;
#if EMULATORDEBUG
        protected readonly PseudoRandomNumberGenerator qRNG;
#else
        protected readonly QuantumRandomNumberGenerator qRNG;
#endif

#if EMULATORDEBUG
        bool barfExceptionsHere = true;
#else
        bool barfExceptionsHere = false;
#endif


        public class CanIgnoreException : Exception { }

        public QuantumRandomNumberGeneratorWrapper(ITurnContext turnContext, MainDialog mainDialog, CancellationToken cancellationToken, string entropyQueryString = null)
        {
            _turnContext = turnContext;
            _mainDialog = mainDialog;
            _cancellationToken = cancellationToken;
#if EMULATORDEBUG
            qRNG = new PseudoRandomNumberGenerator();
#else
            qRNG = new QuantumRandomNumberGenerator();
            qRNG.EntropySrcQueryString = entropyQueryString;
#endif
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

        public byte[] NextHexBytes(int len, int meta, out string shaGid)
        {
            try
            {
                //return File.ReadAllBytes("entropy");
                var res = qRNG.NextHexBytes(len, meta, out shaGid);
                //TODO: delete me! investigating libAttract crash
                //File.WriteAllBytes("entropy" + ((Int32)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds), res);
                return res;
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
            if (barfExceptionsHere)
                return false;

            // Here's a dirty hack (the codebase is starting to get filled with lots of these :-))
            // to catch QRNG source related exceptions to allow us to do operations like
            // send the user a message, reset their scanning flags and take them back to MainDialog prompt
            if ((exception.GetType().Equals(typeof(InvalidDataException)) && "Service did not return random data.".Equals(exception.Message)) ||
                (exception.GetType().Equals(typeof(RuntimeBinderException)) && exception.Message.Contains("does not contain a definition")) ||
                (exception.GetType().Equals(typeof(WebException)))
                )
            {
                _turnContext.SendActivityAsync("Sorry, there was an error sourcing quantum entropy needed to randomize. Try a bit later.").GetAwaiter().GetResult();
                ((AdapterWithErrorHandler)_turnContext.Adapter).RepromptMainDialog(_turnContext, _mainDialog, _cancellationToken, new CallbackOptions() { ResetFlag = true }).GetAwaiter().GetResult();
                return true;
            }

            return false;
        }
    }
}