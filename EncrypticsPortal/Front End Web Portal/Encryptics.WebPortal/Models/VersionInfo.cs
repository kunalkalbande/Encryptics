using System;
using System.Linq;

namespace Encryptics.WebPortal.Models
{
    public class VersionInfo : IComparable<VersionInfo>
    {
        private readonly string _versionNumber;
        private int? _major;
        private int? _minor;
        private int? _patch;
        private int? _build;

        public int Major
        {
            get { return _major ?? 0; }
            private set { _major = value; }
        }

        public int Minor
        {
            get { return _minor ?? 0; }
            private set { _minor = value; }
        }

        public int Patch
        {
            get { return _patch ?? 0; }
            private set { _patch = value; }
        }

        public int Build
        {
            get { return _build ?? 0; }
            private set { _build = value; }
        }

        public VersionInfo(string versionNumber)
        {
            _versionNumber = versionNumber;
            var splitString = versionNumber.Split('.').Select(Int32.Parse).ToArray();

            if (splitString.Length > 3)
            {
                Build = splitString[3];
            }

            if (splitString.Length > 2)
            {
                Patch = splitString[2];
            }

            if (splitString.Length > 1)
            {
                Minor = splitString[1];
            }

            if (splitString.Length > 0)
            {
                Major = splitString[0];
            }
        }

        public int CompareTo(VersionInfo other)
        {
            if (IsGreaterThan(other)) return -1;
            if (Equals(other)) return 0;

            return 1;
        }

        public override string ToString()
        {
            return _versionNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((VersionInfo) obj);
        }

        protected bool Equals(VersionInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            return String.Equals(_versionNumber, other._versionNumber) && Major == other.Major &&
                   Minor == other.Minor && Patch == other.Patch && Build == other.Build;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_versionNumber != null ? _versionNumber.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Major;
                hashCode = (hashCode*397) ^ Minor;
                hashCode = (hashCode*397) ^ Patch;
                hashCode = (hashCode*397) ^ Build;
                return hashCode;
            }
        }

        public static bool operator >(VersionInfo version1, VersionInfo version2)
        {
            return version1.IsGreaterThan(version2);
        }

        public static bool operator <(VersionInfo version1, VersionInfo version2)
        {
            return !version1.IsGreaterThan(version2);
        }

        public static bool operator >=(VersionInfo version1, VersionInfo version2)
        {
            return version1.IsGreaterThan(version2) || version1.Equals(version2);
        }

        public static bool operator <=(VersionInfo version1, VersionInfo version2)
        {
            return !version1.IsGreaterThan(version2) || version1.Equals(version2);
        }

        public static bool operator ==(VersionInfo version1, VersionInfo version2)
        {
            return !ReferenceEquals(null, version1) && version1.Equals(version2);
        }

        public static bool operator !=(VersionInfo version1, VersionInfo version2)
        {
            return !ReferenceEquals(null, version1) && !version1.Equals(version2);
        }

        public bool IsGreaterThan(VersionInfo other)
        {
            if (ReferenceEquals(null, other)) return false;

            return ((Major > other.Major) ||
                    (Major == other.Major && Minor > other.Minor) ||
                    (Major == other.Major && Minor == other.Minor &&
                     Patch > other.Patch) ||
                    (Major == other.Major && Minor == other.Minor &&
                     Patch == other.Patch && Build > other.Build));
        }
    }
}