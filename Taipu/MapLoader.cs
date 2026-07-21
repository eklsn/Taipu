using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Xna.Framework.Graphics;
namespace Taipu
{
    public class MapLoader
    {
        public string mapFile = "";
        public string mapFolder = "";
        public int currentScheme = 0;
        public string lastError = "0";

        public TaipuMap Load(String filePath)
        {
            if (File.Exists(filePath))
            {
                mapFile = filePath;
                mapFolder = Path.GetDirectoryName(filePath);
                string jsonRaw = File.ReadAllText(filePath);
                TaipuMap level = MapParse(jsonRaw);
                return level;

            }
            else
            {
                return new();
            }
        }
        public TaipuMap MapParse(string jsonRaw)
        {
            var docOptions = new JsonDocumentOptions { AllowTrailingCommas = true };
            JsonDocument jsonData = null;
            try
            {
                jsonData = JsonDocument.Parse(jsonRaw, docOptions);
            }
            catch
            {
                Debug.WriteLine("couldn't parse json. returning an empty map to avoid a crash.");
                return new();
            }
            Dictionary<String,Object> validData = null;
            bool isValid = false;
            if (jsonData.RootElement.ValueKind == JsonValueKind.Array)
            {
                JsonArray gdData = JsonNode.Parse(jsonRaw, null, docOptions).AsArray();
                var mapMeta = gdData[0].AsObject();
                if (mapMeta.ContainsKey("audiofile"))
                {
                    validData = ConvertFromGodot(gdData);
                    isValid = true;
                }
                
            }
            if (jsonData.RootElement.ValueKind == JsonValueKind.Object)
            {
                JsonObject monoData = JsonNode.Parse(jsonRaw, null, docOptions).AsObject();
                if (monoData.ContainsKey("audioFile"))
                {
                    validData = monoData.Deserialize<Dictionary<String, Object>>();
                    isValid = true;
                }
            }
            if (isValid)
            {
                if (Convert.ToInt32(validData["schemeVersion"].ToString()) < currentScheme)
                {
                    validData = ConvertFromOld(validData);
                }
                string tempJson = JsonSerializer.Serialize(validData);
                var options = new JsonSerializerOptions { IncludeFields = true };
                TaipuMap readyLevel = (TaipuMap)JsonSerializer.Deserialize(tempJson, typeof(TaipuMap), options);
                return readyLevel;
            }
            else
            {
                Debug.WriteLine("the file is a json, but not a taipu one. returning an empty map to avoid a crash.");
                return new();
            }
        }

        public Dictionary<String,Object> ConvertFromOld(Dictionary<String,Object> old)
        {
            // placeholder here :D
            return old;
        }

        public Dictionary<String,Object> ConvertFromGodot(JsonArray array)
        {
            // Importing the Godot Prototype Taipu level as monoTaipu scheme 0 level
            Dictionary<String,Object> convertedMap = new();
            var mapMeta = array[0].AsObject();
            var mapKeys = array.Skip(1);
            foreach (var meta in mapMeta)
            {
                switch(meta.Key)
                {
                    case "name": convertedMap["songName"] = meta.Value.ToString(); break;
                    case "song_author": convertedMap["songAuthor"] = meta.Value.ToString(); break;
                    case "lvl_author": convertedMap["mapAuthor"] = meta.Value.ToString(); break;
                    case "audiofile": convertedMap["audioFile"] = meta.Value.ToString(); break;
                    case "videofile": convertedMap["videoFile"] = meta.Value.ToString(); break;
                    case "znote": convertedMap["editorsNote"] = meta.Value.ToString(); break;
                    case "z_note": convertedMap["editorsNote"] = meta.Value.ToString(); break;
                    case "imagefile": convertedMap["imageBg"] = meta.Value.ToString(); break;
                    case "img_cover": convertedMap["imageCover"] = meta.Value.ToString(); break;
                    case "pre_ring_time": convertedMap["preRingTime"] = meta.Value.GetValue<Double>(); break;
                    case "ring_time": convertedMap["ringTime"] = meta.Value.GetValue<Double>(); break;
                    case "hit_timeframe": convertedMap["hitTimeframe"] = meta.Value.GetValue<Double>(); break;
                    case "minus_hp_idle": convertedMap["minusHPIdle"] = meta.Value.GetValue<Double>(); break;
                    case "minus_hp_on_miss": convertedMap["minusHPMiss"] = meta.Value.GetValue<Double>(); break;
                }
            }
            if (!convertedMap.ContainsKey("ringTime"))
            {
                convertedMap["ringTime"] = 0.5;
                convertedMap["preRingTime"] = 0.2;
                convertedMap["hitTimeframe"] = 0.2;
            }
            if (!convertedMap.ContainsKey("minusHPIdle"))
            {
                convertedMap["minusHPIdle"] = 0.001;
                convertedMap["minusHPMiss"] = 25;
            }
            if (!convertedMap.ContainsKey("mapAuthor"))
            {
                convertedMap["mapAuthor"] = "Unknown Mapper";
            }
            if (!convertedMap.ContainsKey("songName"))
            {
                convertedMap["songName"] = "Untitled";
            }
            if (!convertedMap.ContainsKey("songAuthor"))
            {
                convertedMap["songAuthor"] = "Unknown Author";
            }
            convertedMap["bpm"] = 0;
            convertedMap["beatOffset"] = 0;
            convertedMap["appearTime"] = convertedMap["preRingTime"];
            convertedMap["disappearTime"] = 0.5;
            convertedMap["schemeVersion"] = 0;
            List<String[]> newKeys = new();
            foreach (var key in mapKeys)
            {
                String[] keyData = new string[2];
                keyData[0] = key[0].GetValue<double>().ToString();
                keyData[1] = key[1].ToString();
                newKeys.Add(keyData);
            }
            convertedMap["keys"] = newKeys;
            return convertedMap;
        }
    }
}
