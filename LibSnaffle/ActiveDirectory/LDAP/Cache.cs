﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace LibSnaffle.ActiveDirectory.LDAP
{
    public class Cache
    {
        private ConcurrentDictionary<string, string[]> _globalCatalogCache;

        private ConcurrentDictionary<string, Label> _idToTypeCache;

        private ConcurrentDictionary<string, string> _machineSidCache;

        private ConcurrentDictionary<string, string> _sidToDomainCache;

        private ConcurrentDictionary<string, string> _valueToIDCache;

        private Cache()
        {
            _valueToIDCache = new ConcurrentDictionary<string, string>();
            _idToTypeCache = new ConcurrentDictionary<string, Label>();
            _globalCatalogCache = new ConcurrentDictionary<string, string[]>();
            _machineSidCache = new ConcurrentDictionary<string, string>();
            _sidToDomainCache = new ConcurrentDictionary<string, string>();
        }

        private static Cache CacheInstance { get; set; }

        /// <summary>
        ///     Add a SID <-> Domain mapping to the cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal static void AddSidToDomain(string key, string value)
        {
            CacheInstance?._sidToDomainCache.TryAdd(key, value);
        }

        /// <summary>
        ///     Get a SID to Domain or Domain to SID mapping
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool GetDomainSidMapping(string key, out string value)
        {
            if (CacheInstance != null) return CacheInstance._machineSidCache.TryGetValue(key, out value);
            value = null;
            return false;
        }

        /// <summary>
        ///     Add a Domain SID -> Computer SID mapping to the cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal static void AddMachineSid(string key, string value)
        {
            CacheInstance?._machineSidCache.TryAdd(key, value);
        }

        internal static bool GetMachineSid(string key, out string value)
        {
            if (CacheInstance != null) return CacheInstance._machineSidCache.TryGetValue(key, out value);
            value = null;
            return false;
        }

        internal static void AddConvertedValue(string key, string value)
        {
            CacheInstance?._valueToIDCache.TryAdd(key, value);
        }

        internal static void AddPrefixedValue(string key, string domain, string value)
        {
            CacheInstance?._valueToIDCache.TryAdd(GetPrefixKey(key, domain), value);
        }

        internal static void AddType(string key, Label value)
        {
            CacheInstance?._idToTypeCache.TryAdd(key, value);
        }

        internal static void AddGCCache(string key, string[] value)
        {
            CacheInstance?._globalCatalogCache?.TryAdd(key, value);
        }

        internal static bool GetGCCache(string key, out string[] value)
        {
            if (CacheInstance != null) return CacheInstance._globalCatalogCache.TryGetValue(key, out value);
            value = null;
            return false;
        }

        internal static bool GetConvertedValue(string key, out string value)
        {
            if (CacheInstance != null) return CacheInstance._valueToIDCache.TryGetValue(key, out value);
            value = null;
            return false;
        }

        internal static bool GetPrefixedValue(string key, string domain, out string value)
        {
            if (CacheInstance != null)
                return CacheInstance._valueToIDCache.TryGetValue(GetPrefixKey(key, domain), out value);
            value = null;
            return false;
        }

        internal static bool GetIDType(string key, out Label value)
        {
            if (CacheInstance != null) return CacheInstance._idToTypeCache.TryGetValue(key, out value);
            value = Label.Base;
            return false;
        }

        private static string GetPrefixKey(string key, string domain)
        {
            return $"{key}|{domain}";
        }

        public static void CreateNewCache()
        {
            CacheInstance = new Cache();
        }

        private static string GetCacheStats()
        {
            try
            {
                return
                    $"{CacheInstance._idToTypeCache.Count} ID to type mappings.\n {CacheInstance._valueToIDCache.Count} name to SID mappings.\n {CacheInstance._machineSidCache.Count} machine sid mappings.\n {CacheInstance._sidToDomainCache.Count} sid to domain mappings.\n {CacheInstance._globalCatalogCache.Count} global catalog mappings.";
            }
            catch
            {
                return "";
            }
        }

        /*
        public static void LoadExistingCache(string filePath)
        {
            if (!File.Exists(filePath))
            {
                CacheInstance = new Cache();
                //Logging.Debug("Cache file not found, empty cache created.");
                return;
            }

            try
            {
                //Logging.Debug($"Loading cache from {filePath}");
                var bytes = File.ReadAllBytes(filePath);
                var json = new UTF8Encoding(true).GetString(bytes);
                CacheInstance = JsonConvert.DeserializeObject<Cache>(json, new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Populate
                });
            }
            catch (Exception e)
            {
                //Logging.Debug($"Exception loading cache: {e}. Creating empty cache.");
                CacheInstance = new Cache();
            }

            CreateMissingDictionaries();

            //Logging.Debug(
                $"Cache file loaded!\n {GetCacheStats()}");
        }
        */

        private static void CreateMissingDictionaries()
        {
            if (CacheInstance == null)
            {
                CacheInstance = new Cache();}

            if (CacheInstance._idToTypeCache == null)
            {
                CacheInstance._idToTypeCache = new ConcurrentDictionary<string, Label>();}

            if (CacheInstance._globalCatalogCache == null)
            {
                CacheInstance._globalCatalogCache = new ConcurrentDictionary<string, string[]>();}

            if (CacheInstance._machineSidCache == null)
            {
                CacheInstance._machineSidCache = new ConcurrentDictionary<string, string>();}

            if (CacheInstance._sidToDomainCache == null)
            {
                CacheInstance._sidToDomainCache = new ConcurrentDictionary<string, string>();}

            if (CacheInstance._valueToIDCache == null)
            {
                CacheInstance._valueToIDCache = new ConcurrentDictionary<string, string>();}
        }
        /*
        public static void SaveCache(string filePath)
        {
            var serialized = new UTF8Encoding(true).GetBytes(JsonConvert.SerializeObject(CacheInstance,
                new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Include
                }));
            using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            stream.Write(serialized, 0, serialized.Length);
            //Logging.Log(LogLevel.Information, $"Wrote cache file to {filePath}\n{GetCacheStats()}");
        }
        */
    }
}