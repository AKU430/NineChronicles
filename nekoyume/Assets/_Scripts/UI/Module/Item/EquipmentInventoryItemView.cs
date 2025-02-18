using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Action;
using Nekoyume.Helper;
using Nekoyume.Model.Item;
using Nekoyume.UI.Model;
using Nekoyume.UI.Scroller;
using UnityEngine;

namespace Nekoyume.UI.Module
{
    using UniRx;

    [RequireComponent(typeof(BaseItemView))]
    public class EquipmentInventoryItemView : MonoBehaviour
    {
        [SerializeField]
        private BaseItemView baseItemView;

        private readonly List<IDisposable> _disposables = new();

        public void Set(EnhancementInventoryItem model, EnhancementInventoryScroll.ContextModel context)
        {
            if (model is null)
            {
                baseItemView.Container.SetActive(false);
                baseItemView.EmptyObject.SetActive(true);
                return;
            }

            _disposables.DisposeAllAndClear();
            baseItemView.ClearItem();
            baseItemView.RuneSelectMove.SetActive(false);
            baseItemView.SelectCollectionObject.SetActive(false);
            baseItemView.SelectArrowObject.SetActive(false);

            baseItemView.ItemImage.overrideSprite = BaseItemView.GetItemIcon(model.ItemBase);

            var data = baseItemView.GetItemViewData(model.ItemBase);
            baseItemView.GradeImage.overrideSprite = data.GradeBackground;
            baseItemView.GradeHsv.range = data.GradeHsvRange;
            baseItemView.GradeHsv.hue = data.GradeHsvHue;
            baseItemView.GradeHsv.saturation = data.GradeHsvSaturation;
            baseItemView.GradeHsv.value = data.GradeHsvValue;

            if (model.ItemBase is Equipment equipment && equipment.level > 0)
            {
                baseItemView.EnhancementText.gameObject.SetActive(true);
                baseItemView.EnhancementText.text = $"+{equipment.level}";
                if (equipment.level >= Util.VisibleEnhancementEffectLevel)
                {
                    baseItemView.EnhancementImage.material = data.EnhancementMaterial;
                    baseItemView.EnhancementImage.gameObject.SetActive(true);
                }
                else
                {
                    baseItemView.EnhancementImage.gameObject.SetActive(false);
                }
            }
            else
            {
                baseItemView.EnhancementText.gameObject.SetActive(false);
                baseItemView.EnhancementImage.gameObject.SetActive(false);
            }

            baseItemView.OptionTag.Set(model.ItemBase);

            model.Equipped.Subscribe(b => baseItemView.EquippedObject.SetActive(b)).AddTo(_disposables);
            model.LevelLimited.Subscribe(b => baseItemView.LevelLimitObject.SetActive(b)).AddTo(_disposables);
            model.Selected.Subscribe(b => baseItemView.SelectObject.SetActive(b)).AddTo(_disposables);
            model.SelectedBase.Subscribe(b => baseItemView.SelectBaseItemObject.SetActive(b)).AddTo(_disposables);
            model.SelectedMaterialCount
                .Subscribe(count => baseItemView.SelectMaterialItemObject.SetActive(count > 0))
                .AddTo(_disposables);
            model.Disabled.Subscribe(b => baseItemView.DimObject.SetActive(b)).AddTo(_disposables);

            if (ItemEnhancement.HammerIds.Contains(model.ItemBase.Id))
            {
                baseItemView.CountText.gameObject.SetActive(true);
                model.SelectedMaterialCount.Subscribe(selected =>
                {
                    var count = model.Count.Value - selected;
                    baseItemView.CountText.text = count.ToString();
                }).AddTo(_disposables);
            }
            else
            {
                baseItemView.CountText.gameObject.SetActive(false);
            }

            baseItemView.TouchHandler.OnClick.Select(_ => model)
                .Subscribe(context.OnClick.OnNext).AddTo(_disposables);

            baseItemView.TouchHandler.OnDoubleClick.Select(_ => model)
                .Subscribe(context.OnDoubleClick.OnNext).AddTo(_disposables);

            if (model.ItemBase is Equipment equipmentItem)
            {
                baseItemView.CustomCraftArea.SetActive(equipmentItem.ByCustomCraft);
            }
        }
    }
}
