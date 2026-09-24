using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Enums;

namespace BillsSystem.Domain.Helpers
{
    public static class EgyptianMobilePrefixHelper
    {
        public static readonly IReadOnlyList<string> AllPrefixStrings =
            Enum.GetValues<EgyptianMobilePrefix>()
                .Select(p => "0" + (int)p)
                .ToList();

        public static bool TryParsePrefix(string phoneNumber, out EgyptianMobilePrefix prefix)
        {
            prefix = default;

            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 3)
                return false;

            var prefixDigits = phoneNumber.Substring(1, 2);
            if (!int.TryParse(prefixDigits, out var value))
                return false;

            if (!Enum.IsDefined(typeof(EgyptianMobilePrefix), value))
                return false;

            prefix = (EgyptianMobilePrefix)value;
            return phoneNumber.StartsWith("0" + value);
        }

        public static bool HasValidPrefix(string phoneNumber) => TryParsePrefix(phoneNumber, out _);

        public static string GetNetworkName(EgyptianMobilePrefix prefix) => prefix switch
        {
            EgyptianMobilePrefix.Vodafone => "Vodafone",
            EgyptianMobilePrefix.Etisalat => "Etisalat",
            EgyptianMobilePrefix.Orange => "Orange",
            EgyptianMobilePrefix.WE => "WE",
            _ => "Unknown"
        };

        public static string? GetNetworkName(string phoneNumber) =>
            TryParsePrefix(phoneNumber, out var prefix) ? GetNetworkName(prefix) : null;
    }
}
