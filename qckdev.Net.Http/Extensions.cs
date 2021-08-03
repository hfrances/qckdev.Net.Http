#if NO_SYNC
#else
using System.Net;

namespace qckdev.Net.Http
{
    static class Extensions
    {

        /// <summary>
        /// Returns the content of a HTTP response message.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static string GetContentAsString(this HttpWebResponse response)
        {
            string rdo;

            using (var responseStream = response.GetResponseStream())
            {
                var charset = System.Text.Encoding.GetEncoding(response.CharacterSet);

                using (var reader = new System.IO.StreamReader(responseStream, charset))
                {
                    rdo = reader.ReadToEnd();
                }
            }
            return rdo;
        }

        /// <summary>
        /// Gets a value that indicates whether the HTTP response was successful.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>
        /// A value that indicates whether the HTTP response was successful. 
        /// true if <see cref="HttpStatusCode"/> is in the Successful range (200-299); otherwise, false.
        /// </returns>
        public static bool IsSuccessStatusCode(this HttpWebResponse response)
        {
            var statusCode = (int)response.StatusCode;

            return statusCode >= 200 && statusCode < 300;
        }

    }
}
#endif