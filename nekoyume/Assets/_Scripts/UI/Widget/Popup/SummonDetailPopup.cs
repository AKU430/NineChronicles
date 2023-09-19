using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Helper;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using Nekoyume.TableData;
using Nekoyume.TableData.Summon;
using Nekoyume.UI.Module;
using Nekoyume.UI.Scroller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    using UniRx;

    public class SummonDetailPopup : PopupWidget
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI titleText;

        [SerializeField] private TextMeshProUGUI[] mainStatTexts;
        [SerializeField] private RecipeOptionView recipeOptionView;

        [SerializeField] private SummonDetailScroll scroll;

        private readonly List<IDisposable> _disposables = new();

        private const string StatTextFormat = "{0} {1}";

        protected override void Awake()
        {
            base.Awake();

            closeButton.onClick.AddListener(() =>
            {
                Close();
            });
            CloseWidget = () =>
            {
                Close(true);
            };
        }

        public void Show(SummonSheet.Row summonRow)
        {
            var tableSheets = Game.Game.instance.TableSheets;
            var equipmentItemSheet = tableSheets.EquipmentItemSheet;
            var equipmentItemRecipeSheet = tableSheets.EquipmentItemRecipeSheet;
            var equipmentItemSubRecipeSheet = tableSheets.EquipmentItemSubRecipeSheetV2;

            float ratioSum = summonRow.Recipes.Sum(pair => pair.Item2);
            var models = summonRow.Recipes.Where(pair => pair.Item1 > 0).Select(pair =>
            {
                var recipeRow = equipmentItemRecipeSheet[pair.Item1];
                return new SummonDetailCell.Model
                {
                    EquipmentRow = equipmentItemSheet[recipeRow.ResultEquipmentId],
                    Options = equipmentItemSubRecipeSheet[recipeRow.SubRecipeIds[0]].Options,
                    Ratio = pair.Item2 / ratioSum,
                };
            }).OrderBy(model => model.Ratio);

            _disposables.DisposeAllAndClear();
            scroll.OnClick.Subscribe(PreviewDetail).AddTo(_disposables);
            scroll.UpdateData(models, true);

            titleText.text = summonRow.GetLocalizedName();
            base.Show();
        }

        private void PreviewDetail(SummonDetailCell.Model model)
        {
            // CharacterView
            SetCharacter(model.EquipmentRow);

            // MainStatText
            var mainStatText = mainStatTexts[0];
            var stat = model.EquipmentRow.GetUniqueStat();
            var statValueText = stat.StatType.ValueToString((int)stat.TotalValue);
            mainStatText.text = string.Format(StatTextFormat, stat.StatType, statValueText);
            mainStatText.gameObject.SetActive(true);

            // OptionView
            recipeOptionView.SetOptions(model.Options, false);
        }

        private static void SetCharacter(EquipmentItemSheet.Row equipmentRow)
        {
            var game = Game.Game.instance;

            var maxLevel = game.TableSheets.EnhancementCostSheetV3.Values
                .Where(row =>
                    row.ItemSubType == equipmentRow.ItemSubType &&
                    row.Grade == equipmentRow.Grade)
                .Max(row => row.Level);
            var resultItem = (Equipment)ItemFactory.CreateItemUsable(
                    equipmentRow, Guid.NewGuid(), 0L, maxLevel);

            var (equipments, costumes) = game.States.GetEquippedItems(BattleType.Adventure);
            var sameType = equipments.FirstOrDefault(e => e.ItemSubType == equipmentRow.ItemSubType);
            equipments.Remove(sameType);
            equipments.Add(resultItem);

            var avatarState = game.States.CurrentAvatarState;
            game.Lobby.FriendCharacter.Set(avatarState, costumes, equipments);
        }
    }
}
