using System;
using System.Collections.Generic;
using Nekoyume.Model.Item;
using UniRx;
using UnityEngine.UI;

namespace Nekoyume.UI.Module
{
    public class InventoryItemView : CountableItemView<Model.InventoryItem>
    {
        public Image effectImage;
        public Image glowImage;
        public Image equippedIcon;
        public Image hasNotificationImage;
        public Image nonTradableIcon;

        protected override ImageSizeType imageSizeType => ImageSizeType.Middle;

        private readonly List<IDisposable> _disposablesAtSetData = new List<IDisposable>();

        #region Mono

        protected override void OnDestroy()
        {
            Clear();
        }

        #endregion

        #region override

        public override void SetData(Model.InventoryItem model)
        {
            if (model is null)
            {
                Clear();
                return;
            }

            base.SetData(model);
            _disposablesAtSetData.DisposeAllAndClear();
            Model.EffectEnabled.SubscribeTo(effectImage).AddTo(_disposablesAtSetData);
            Model.GlowEnabled.SubscribeTo(glowImage).AddTo(_disposablesAtSetData);
            Model.EquippedEnabled.SubscribeTo(equippedIcon).AddTo(_disposablesAtSetData);
            Model.HasNotification.SubscribeTo(hasNotificationImage).AddTo(_disposablesAtSetData);
            Model.IsTradable.Value = model.ItemBase.Value is ITradableItem;
            Model.View = this;
            UpdateView();
            Model.ActiveSelf.Subscribe(SetActive).AddTo(_disposablesAtSetData);
        }

        private void SetActive(bool value)
        {
            if (!value)
            {
                Clear();
            }
        }

        public override void Clear()
        {
            _disposablesAtSetData.DisposeAllAndClear();
            base.Clear();

            UpdateView();
        }

        protected override void SetDim(bool isDim)
        {
            base.SetDim(isDim);

            effectImage.color = isDim ? DimmedColor : OriginColor;
            glowImage.color = isDim ? DimmedColor : OriginColor;
            equippedIcon.color = isDim ? DimmedColor : OriginColor;
            hasNotificationImage.color = isDim ? DimmedColor : OriginColor;
        }

        #endregion

        private void UpdateView()
        {
            if (Model is null)
            {
                effectImage.enabled = false;
                glowImage.enabled = false;
                equippedIcon.enabled = false;
                hasNotificationImage.enabled = false;
                nonTradableIcon.enabled = false;
                return;
            }

            effectImage.enabled = Model.EffectEnabled.Value;
            glowImage.enabled = Model.GlowEnabled.Value;
            equippedIcon.enabled = Model.EquippedEnabled.Value;
            hasNotificationImage.enabled = Model.HasNotification.Value;
            nonTradableIcon.enabled = !Model.IsTradable.Value;
        }
    }
}
