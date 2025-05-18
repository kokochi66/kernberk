using System.Collections.Generic;
using UnityEngine;
using System.IO;
using NPOI.XSSF.UserModel; // .xlsx 전용
using NPOI.SS.UserModel;
using System;

namespace Story.Core
{
    public static class VNEventParser
    {
        public static List<VNEvent> LoadEventsFromExcelPath(string fullPath)
        {
            List<VNEvent> events = new List<VNEvent>();

            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet sheet = workbook.GetSheetAt(0); // 첫 번째 시트

                var headerRow = sheet.GetRow(0);
                var columnIndexMap = new Dictionary<string, int>();
                for (int i = 0; i < headerRow.LastCellNum; i++)
                {
                    var cell = headerRow.GetCell(i);
                    if (cell != null && !string.IsNullOrWhiteSpace(cell.ToString()))
                        columnIndexMap[cell.ToString().Trim()] = i;
                }

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue;

                    VNEvent e = new VNEvent
                    {
                        sceneName = GetCell(row, columnIndexMap, "sceneName"),
                        eventIndex = int.TryParse(GetCell(row, columnIndexMap, "eventIndex"), out int idx) ? idx : 0,
                        characterName = GetCell(row, columnIndexMap, "characterName"),
                        dialogue = GetCell(row, columnIndexMap, "dialogue"),
                        delayBefore = float.TryParse(GetCell(row, columnIndexMap, "delayBefore"), out float delay) ? delay : 0f,
                        gameObjectNames = ParseList(GetCell(row, columnIndexMap, "gameObjectNames")),
                        positions = ParsePositions(GetCell(row, columnIndexMap, "positions")),
                        fadeType = ParseEnum<FadeType>(GetCell(row, columnIndexMap, "fadeType")),
                        expression = GetCell(row, columnIndexMap, "expression"),
                        sceneFadeType = ParseEnum<SceneFadeType>(GetCell(row, columnIndexMap, "sceneFadeType")),
                        sfx = GetCell(row, columnIndexMap, "sfx"),
                        bgm = GetCell(row, columnIndexMap, "bgm"),
                        bgmVolume = float.TryParse(GetCell(row, columnIndexMap, "bgmVolume"), out float volume) ? volume : 0,
                        bgImage = GetCell(row, columnIndexMap, "bgImage")
                    };

                    events.Add(e);
                }
            }

            return events;
        }


        private static string GetCell(IRow row, Dictionary<string, int> map, string key)
        {
            if (!map.ContainsKey(key)) return "";
            var cell = row.GetCell(map[key]);
            return cell == null ? "" : cell.ToString().Trim();
        }

        private static List<string> ParseList(string str)
        {
            return new List<string>(str.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
        }

        private static List<VNPosition> ParsePositions(string str)
        {
            var raw = str.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            var result = new List<VNPosition>();
            foreach (var s in raw)
            {
                if (System.Enum.TryParse(s.Trim(), true, out VNPosition pos))
                    result.Add(pos);
            }
            return result;
        }

        private static T ParseEnum<T>(string str) where T : struct
        {
            if (System.Enum.TryParse<T>(str.Trim(), true, out T val))
                return val;
            return default;
        }
    }
}
