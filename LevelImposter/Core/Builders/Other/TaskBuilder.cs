using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine;
using LevelImposter.DB;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LevelImposter.Core
{
    public class TaskBuilder : IElemBuilder
    {
        public static readonly Dictionary<string, TaskLength> TASK_LENGTHS = new()
        {
            { "Short", TaskLength.Short },
            { "Long", TaskLength.Long },
            { "Common", TaskLength.Common }
        };
        public static readonly Dictionary<string, int> CONSOLE_ID_PAIRS = new()
        {
            { "task-garbage2", 1 },
            { "task-garbage3", 0 },
            { "task-garbage4", 2 },
            { "task-fans1", 0 },
            { "task-fans2", 1 },
            { "task-records1", 0 },
            { "task-pistols1", 1 },
            { "task-pistols2", 1 }
        };
        public static readonly Dictionary<string, int> CONSOLE_ID_INCREMENTS = new()
        {
            { "task-toilet", 0 },
            { "task-breakers", 0 },
            { "task-towels", 0 },
            { "task-node", 0 },
            { "task-waterwheel1", 0 },
            { "task-fuel2", 0 },
            { "task-align1", 0 },
            { "task-records2", 1 },
            { "task-wires", 0 }
        };

        private GameObject? _taskContainer = null;
        private NormalPlayerTask? _wiresTask = null;
        private int _consoleID = 0;
        private List<string> _builtTypes = new();
        private Dictionary<string, int> _consoleIDIncrements = new(CONSOLE_ID_INCREMENTS);

        private static SystemTypes[] _divertSystems = Array.Empty<SystemTypes>();
        public static SystemTypes[] DivertSystems => _divertSystems;

        private static byte _breakerCount = 0;
        private static byte _toiletCount = 0;
        private static byte _towelCount = 0;
        private static byte _fuelCount = 0;
        private static byte _recordsCount = 0;
        private static byte _alignEngineCount = 0;
        private static byte _waterWheelCount = 0;
        private static byte _wiresCount = 0;

        public static byte BreakerCount => _breakerCount;
        public static byte ToiletCount => _toiletCount;
        public static byte TowelCount => _towelCount;
        public static byte FuelCount => _fuelCount;
        public static byte WaterWheelCount => _waterWheelCount;
        public static byte AlignEngineCount => _alignEngineCount;
        public static byte RecordsCount => _recordsCount;
        public static byte WiresCount => _wiresCount;

        private static int MIN_SHORT_TASKS = 3;
        private static int MIN_COMMON_TASKS = 2;
        private static int MIN_LONG_TASKS = 1;

        public TaskBuilder()
        {
            _divertSystems = Array.Empty<SystemTypes>();
            _breakerCount = 0;
            _toiletCount = 0;
            _towelCount = 0;
            _fuelCount = 0;
            _waterWheelCount = 0;
            _alignEngineCount = 0;
            _recordsCount = 0;
            _wiresCount = 0;
    }

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("task-"))
                return;

            // ShipStatus
            if (LIShipStatus.Instance?.ShipStatus == null)
                throw new Exception("ShipStatus not found");
            ShipStatus shipStatus = LIShipStatus.Instance.ShipStatus;

            // Create Task Container
            if (_taskContainer == null)
            {
                _taskContainer = new GameObject("Tasks");
                _taskContainer.transform.SetParent(LIShipStatus.Instance.transform);
            }

            // Prefab
            var prefab = AssetDB.GetObject(elem.type);
            if (prefab == null)
                return;
            var prefabRenderer = prefab.GetComponent<SpriteRenderer>();
            var prefabConsole = prefab.GetComponentInChildren<Console>();
            var prefabBtn = prefab.GetComponentInChildren<PassiveButton>();

            // Default Sprite
            SpriteRenderer spriteRenderer = MapUtils.CloneSprite(obj, prefab);

            // Parent
            SystemTypes systemType = RoomBuilder.GetParentOrDefault(elem);

            bool isArms = elem.type == "task-pistols1" || elem.type == "task-rifles1";
            bool isTowel = elem.type.StartsWith("task-towels") && elem.type != "task-towels1";

            // Console
            Console console;
            if (isArms)
            {
                console = obj.AddComponent<StoreArmsTaskConsole>();
                StoreArmsTaskConsole specialConsole = console.Cast<StoreArmsTaskConsole>();
                StoreArmsTaskConsole origSpecialConsole = prefabConsole.Cast<StoreArmsTaskConsole>();

                specialConsole.timesUsed = origSpecialConsole.timesUsed;
                specialConsole.Images = origSpecialConsole.Images;
                specialConsole.useSound = origSpecialConsole.useSound;
                specialConsole.usesPerStep = origSpecialConsole.usesPerStep;
            }
            else if (isTowel)
            {
                console = obj.AddComponent<TowelTaskConsole>();
                TowelTaskConsole specialConsole = console.Cast<TowelTaskConsole>();
                TowelTaskConsole origSpecialConsole = prefabConsole.Cast<TowelTaskConsole>();

                specialConsole.useSound = origSpecialConsole.useSound;
            }
            else
            {
                console = obj.AddComponent<Console>();
            }
            console.ConsoleId = _consoleID;
            console.Image = spriteRenderer;
            console.onlyFromBelow = elem.properties.onlyFromBelow == true;
            console.usableDistance = elem.properties.range == null ? 1.0f : (float)elem.properties.range;
            console.Room = systemType;
            console.TaskTypes = prefabConsole.TaskTypes;
            console.ValidTasks = prefabConsole.ValidTasks;
            console.AllowImpostor = false;
            console.checkWalls = elem.properties.checkCollision ?? false;


            bool isWaterJug = elem.type == "task-waterjug2";
            bool isTowels = elem.type.StartsWith("task-towels");
            bool isFuel = elem.type == "task-fuel1";
            bool isFuelOutput = elem.type == "task-fuel2";

            if (CONSOLE_ID_PAIRS.ContainsKey(elem.type))
            {
                console.ConsoleId = CONSOLE_ID_PAIRS[elem.type];
            }
            else if (isWaterJug)
            {
                TaskSet taskSet = new()
                {
                    taskType = TaskTypes.ReplaceWaterJug,
                    taskStep = new IntRange(1, 1)
                };
                console.ValidTasks = new Il2CppReferenceArray<TaskSet>(new TaskSet[] {
                    taskSet
                });
            }
            else if (isTowels)
            {
                if (elem.type == "task-towels1")
                    console.ConsoleId = 255;
                else
                {
                    console.ConsoleId = _consoleIDIncrements["task-towels"];
                    _consoleIDIncrements["task-towels"]++;
                }
            }
            else if (isFuel)
            {
                console.ValidTasks = new Il2CppReferenceArray<TaskSet>(byte.MaxValue / 2);
                for (byte i = 0; i < byte.MaxValue - 1; i+=2)
                {
                    TaskSet taskSet = new()
                    {
                        taskType = TaskTypes.FuelEngines,
                        taskStep = new(i, i)
                    };
                    console.ValidTasks[i / 2] = taskSet;
                }
            }
            else if (isFuelOutput)
            {
                console.ConsoleId = _consoleIDIncrements[elem.type];
                TaskSet taskSet = new()
                {
                    taskType = TaskTypes.FuelEngines,
                    taskStep = new(console.ConsoleId * 2 + 1, console.ConsoleId * 2 + 1)
                };
                console.ValidTasks = new Il2CppReferenceArray<TaskSet>(new TaskSet[] {
                    taskSet
                });
                _consoleIDIncrements[elem.type]++;
            }
            else if (_consoleIDIncrements.ContainsKey(elem.type))
            {
                console.ConsoleId = _consoleIDIncrements[elem.type];
                _consoleIDIncrements[elem.type]++;
            }
            else
            {
                console.ConsoleId = _consoleID;
                _consoleID++;
            }

            // Colliders
            MapUtils.CreateDefaultColliders(obj, prefab);

            // Button
            if (prefabBtn != null)
            {
                PassiveButton btn = obj.AddComponent<PassiveButton>();
                btn.ClickMask = obj.GetComponent<Collider2D>();
                btn.OnMouseOver = new UnityEvent();
                btn.OnMouseOut = new UnityEvent();
                Action action = console.Use;
                btn.OnClick.AddListener(action);
            }

            // Task
            bool isBuilt = _builtTypes.Contains(elem.type);
            _builtTypes.Add(elem.type);

            if (!isBuilt)
                LILogger.Info($" + Adding task for {elem}...");


            // TODO: Clean this spaghetti mess
            if (elem.type == "task-divert1" && !isBuilt)
            {
                // Prefab
                var prefabTask = AssetDB.GetTask<NormalPlayerTask>(elem.type);
                var prefabArrow = prefabTask?.Arrow?.gameObject;
                var prefabLength = AssetDB.GetTaskLength(elem.type);

                List<LIElement> divertTargets = new();
                if (LIShipStatus.Instance.CurrentMap == null)
                    throw new Exception("Current map is unavailable");
                foreach (LIElement mapElem in LIShipStatus.Instance.CurrentMap.elements)
                    if (mapElem.type == "task-divert2")
                        divertTargets.Add(mapElem);

                _divertSystems = new SystemTypes[divertTargets.Count];
                for (int i = 0; i < divertTargets.Count; i++)
                {
                    LIElement divertTarget = divertTargets[i];

                    SystemTypes divertSystem = RoomBuilder.GetParentOrDefault(divertTarget);
                    _divertSystems[i] = divertSystem;

                    GameObject taskHolder = new(elem.name);
                    taskHolder.transform.SetParent(_taskContainer.transform);

                    DivertPowerTask task = taskHolder.AddComponent<DivertPowerTask>();
                    task.StartAt = systemType;
                    task.taskStep = prefabTask.taskStep;
                    task.MaxStep = prefabTask.MaxStep;
                    task.arrowSuspended = prefabTask.arrowSuspended;
                    task.ShowTaskTimer = prefabTask.ShowTaskTimer;
                    task.ShowTaskStep = prefabTask.ShowTaskStep;
                    task.TaskTimer = prefabTask.TaskTimer;
                    task.TimerStarted = prefabTask.TimerStarted;
                    task.TaskType = prefabTask.TaskType;
                    task.MinigamePrefab = prefabTask.MinigamePrefab;
                    task.TargetSystem = divertSystem;

                    if (prefabArrow != null)
                    {
                        GameObject arrow = UnityEngine.Object.Instantiate(prefabArrow);
                        arrow.transform.SetParent(task.transform);
                        arrow.SetActive(false);
                        task.Arrow = arrow.GetComponent<ArrowBehaviour>();
                    }

                    AddTaskToShip(elem, shipStatus, prefabLength, task);
                }
            }
            else if (AssetDB.HasTask(elem.type) && !isBuilt)
            {
                // Prefab
                var prefabTask = AssetDB.GetTask<NormalPlayerTask>(elem.type);
                var prefabArrow = prefabTask?.Arrow?.gameObject;
                var prefabLength = AssetDB.GetTaskLength(elem.type);

                if (!string.IsNullOrEmpty(elem.properties.description))
                    MapUtils.Rename(prefabTask.TaskType, elem.properties.description);

                GameObject taskHolder = new(elem.name);
                taskHolder.transform.SetParent(_taskContainer.transform);

                NormalPlayerTask task = taskHolder.AddComponent(prefabTask.GetIl2CppType()).Cast<NormalPlayerTask>();
                task.StartAt = systemType;
                task.taskStep = prefabTask.taskStep;
                task.MaxStep = prefabTask.MaxStep;
                task.arrowSuspended = prefabTask.arrowSuspended;
                task.ShowTaskTimer = prefabTask.ShowTaskTimer;
                task.ShowTaskStep = prefabTask.ShowTaskStep;
                task.TaskTimer = prefabTask.TaskTimer;
                task.TimerStarted = prefabTask.TimerStarted;
                task.TaskType = prefabTask.TaskType;
                task.MinigamePrefab = prefabTask.MinigamePrefab;

                if (prefabArrow != null)
                {
                    GameObject arrow = UnityEngine.Object.Instantiate(prefabArrow);
                    arrow.transform.SetParent(task.transform);
                    arrow.SetActive(false);

                    task.Arrow = arrow.GetComponent<ArrowBehaviour>();
                }

                if (elem.type == "task-node")
                {
                    WeatherNodeTask nodeTask = task.Cast<WeatherNodeTask>();
                    nodeTask.NodeId = console.ConsoleId;
                    nodeTask.Stage2Prefab = prefabTask.Cast<WeatherNodeTask>().Stage2Prefab;
                }

                if (elem.type == "task-wires")
                {
                    _wiresTask = task;
                }

                AddTaskToShip(elem, shipStatus, prefabLength, task);
            }

            // Medscan
            bool isMedscan = elem.type == "task-medscan";
            if (isMedscan)
            {
                if (shipStatus.MedScanner != null)
                    LILogger.Warn("Only 1 med scanner can be used per map");
                MedScannerBehaviour medscan = obj.AddComponent<MedScannerBehaviour>();
                shipStatus.MedScanner = medscan;
            }
        }

        private void AddTaskToShip(
            LIElement elem,
            ShipStatus shipStatus,
            TaskLength prefabLength,
            NormalPlayerTask task)
        {
            string? taskLengthProp = elem.properties.taskLength;
            TaskLength taskLength = taskLengthProp != null ? TASK_LENGTHS[taskLengthProp] : prefabLength;
            if (taskLength == TaskLength.Common)
                shipStatus.CommonTasks = MapUtils.AddToArr(shipStatus.CommonTasks, task);
            if (taskLength == TaskLength.Short)
                shipStatus.NormalTasks = MapUtils.AddToArr(shipStatus.NormalTasks, task);
            if (taskLength == TaskLength.Long)
                shipStatus.LongTasks = MapUtils.AddToArr(shipStatus.LongTasks, task);
        }

        public void PostBuild() {
            string[] keys = new string[_consoleIDIncrements.Keys.Count];
            _consoleIDIncrements.Keys.CopyTo(keys, 0);

            foreach (var key in keys)
            {
                byte count = (byte)_consoleIDIncrements[key];
                if (key == "task-breakers")
                    _breakerCount = count;
                if (key == "task-toilet")
                    _toiletCount = count;
                if (key == "task-towels")
                    _towelCount = count;
                if (key == "task-fuel2")
                    _fuelCount = count;
                if (key == "task-waterwheel1")
                    _waterWheelCount = count;
                if (key == "task-align1")
                    _alignEngineCount = count;
                if (key == "task-records2")
                    _recordsCount = count;
                if (key == "task-wires")
                    _wiresCount = count;
                _consoleIDIncrements[key] = 0;
            }

            // Wires Length
            if (_wiresTask != null)
            {
                _wiresTask.MaxStep = Math.Min(WiresCount, (byte)3);
            }
            _wiresTask = null;

            // Check Task Counts
            ShipStatus? shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus != null)
            {
                if (shipStatus.NormalTasks.Count < MIN_SHORT_TASKS)
                    LILogger.Warn($"Freeplay requires {MIN_SHORT_TASKS} short tasks (Currently has {shipStatus.NormalTasks.Count})");

                if (shipStatus.CommonTasks.Count < MIN_COMMON_TASKS)
                    LILogger.Warn($"Freeplay requires {MIN_COMMON_TASKS} common tasks (Currently has {shipStatus.CommonTasks.Count})");

                if (shipStatus.LongTasks.Count < MIN_LONG_TASKS)
                    LILogger.Warn($"Freeplay requires {MIN_LONG_TASKS} long tasks (Currently has {shipStatus.LongTasks.Count})");

            }
        }
    }
}
