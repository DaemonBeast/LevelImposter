﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      By default, various tasks have hard-coded
     *      counts and limits. In order to allow for
     *      flexibility, this has been patched here.
     */
    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.Initialize))]
    public static class TaskInitializePostPatch
    {
        public static void Postfix(NormalPlayerTask __instance)
        {
            if (MapLoader.CurrentMap == null)
                return;

            TaskTypes taskType = __instance.TaskType;
            switch (taskType)
            {
                case TaskTypes.ResetBreakers:
                    __instance.Data = new byte[TaskBuilder.BreakerCount];
                    for (byte i = 0; i < TaskBuilder.BreakerCount; i++)
                        __instance.Data[i] = i;
                    __instance.Data = MapUtils.Shuffle(__instance.Data);
                    __instance.MaxStep = TaskBuilder.BreakerCount;
                    break;
                case TaskTypes.CleanToilet:
                    __instance.Data = new byte[1];
                    __instance.Data[0] = IntRange.NextByte(0, TaskBuilder.ToiletCount);
                    break;
                case TaskTypes.PickUpTowels:
                    __instance.Data = new byte[TaskBuilder.TowelCount / 2];
                    byte[] tempData = new byte[TaskBuilder.TowelCount];
                    for (byte i = 0; i < TaskBuilder.TowelCount; i++)
                        tempData[i] = i;
                    tempData = MapUtils.Shuffle(tempData);
                    for (byte i = 0; i < __instance.Data.Count; i++)
                        __instance.Data[i] = tempData[i];
                    break;
                case TaskTypes.FuelEngines:
                    __instance.MaxStep = TaskBuilder.FuelCount;
                    break;
                case TaskTypes.AlignEngineOutput:
                    __instance.Data = new byte[TaskBuilder.AlignEngineCount + 2];
                    for (int i = 0; i < TaskBuilder.AlignEngineCount; i++)
                        __instance.Data[i] = (byte)(IntRange.RandomSign() * IntRange.Next(25, 127) + 127);
                    __instance.MaxStep = TaskBuilder.AlignEngineCount;
                    break;
            }
        }
    }

    /*
     *      Of course, WaterWayTask has to
     *      override NormalPlayerTask for no reason...
     */
    [HarmonyPatch(typeof(WaterWayTask), nameof(WaterWayTask.Initialize))]
    public static class WaterWheelInitializePatch
    {
        public static void Postfix(NormalPlayerTask __instance)
        {
            if (MapLoader.CurrentMap == null)
                return;

            __instance.Data = new byte[TaskBuilder.WaterWheelCount];
            __instance.MaxStep = TaskBuilder.WaterWheelCount;
        }
    }

    /*
     *      Fix Divert Power Sliders
     */
    [HarmonyPatch(typeof(DivertPowerMinigame), nameof(DivertPowerMinigame.Begin))]
    public static class DivertBeginPatch
    {
        public static void Prefix([HarmonyArgument(0)] PlayerTask task, DivertPowerMinigame __instance)
        {
            if (MapLoader.CurrentMap == null)
                return;

            __instance.SliderOrder = TaskBuilder.DivertSystems;
        }
    }

    /*
     *      Fix Fuel Stages
     */
    [HarmonyPatch(typeof(MultistageMinigame), nameof(MultistageMinigame.Begin))]
    public static class MinigameBeginPatch
    {
        public static bool Prefix([HarmonyArgument(0)] PlayerTask task, MultistageMinigame __instance)
        {
            if (MapLoader.CurrentMap == null)
                return true;

            NormalPlayerTask normalPlayerTask = task.Cast<NormalPlayerTask>();
            if (normalPlayerTask.TaskType == TaskTypes.FuelEngines)
            {
                __instance.stage = __instance.Stages[normalPlayerTask.Data[1] % 2];
                __instance.stage.gameObject.SetActive(true);
                __instance.stage.Begin(task);
                Minigame.Instance = __instance;
                UiElement defaultSelection = null;
                foreach (UiElement uiElement in __instance.ControllerSelectable)
                {
                    if (uiElement.isActiveAndEnabled)
                    {
                        defaultSelection = uiElement;
                        break;
                    }
                }
                ControllerManager.Instance.OpenOverlayMenu(
                    __instance.name,
                    __instance.BackButton,
                    defaultSelection,
                    __instance.ControllerSelectable,
                    false
                );
                return false;
            }
            return true;
        }
    }

    /*
     *      Fix Records Stages
     */
    [HarmonyPatch(typeof(RecordsMinigame), nameof(RecordsMinigame.GrabFolder))]
    public static class RecordsPatch
    {
        public static bool Prefix([HarmonyArgument(0)] SpriteRenderer folder, RecordsMinigame __instance)
        {
            if (MapLoader.CurrentMap == null)
                return true;

            if (__instance.amClosing != Minigame.CloseState.None)
                return false;

            int folderIndex = 0;
            for (int i = 0; i < __instance.Folders.Count; i++)
            {
                if (__instance.Folders[i].gameObject == folder.gameObject)
                    folderIndex = i;
            }

            folder.gameObject.SetActive(false);
            __instance.MyNormTask.Data[folderIndex] = IntRange.NextByte(1, TaskBuilder.RecordsCount);
            __instance.MyNormTask.UpdateArrow();
            if (Constants.ShouldPlaySfx())
                SoundManager.Instance.PlaySound(__instance.grabDocument, false, 1f);
            __instance.StartCoroutine(__instance.CoStartClose(0.75f));

            return false;
        }
    }
}