using System.Collections.Generic;

namespace Encryptics.WebPortal.Models
{
    public struct TokenStatus
    {
        public static readonly int NoError = 0;
        public static readonly int Unknown = 200;
        public static readonly int Exception = 201;
        public static readonly int Expired = 202;
        public static readonly int MissingToken = 203;
        public static readonly int BadToken = 204;
        public static readonly int BadHardwareId = 205;
        public static readonly int BadLocation = 206;
        public static readonly int BadRequest = 207;
        public static readonly int ExpiredByUser = 208;
        public static readonly int Succes = 100;
        public static readonly int NewToken = 101;

        public static readonly Dictionary<int, string> ErrorMessageDictionary = new Dictionary<int, string>
            {
                {NoError, "No Error"},
                {Unknown, "Unknown error"},
                {Exception, "Exception thrown"},
                {Expired, "Token expired"},
                {MissingToken, "Token missing"},
                {BadToken, "Bad token"},
                {BadHardwareId, "Bad hardware Id"},
                {BadLocation, "Bad location"},
                {BadRequest, "Bar request"},
                {ExpiredByUser, "Toekn expired by User"},
                {Succes, "Success"},
                {NewToken, "New Token Issued"}
            };
    }
}