using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
// ReSharper disable MemberCanBePrivate.Global

namespace SpectrumEngine.Client.Avalonia.Controls.Common;

/// <summary>
/// A control with a header that has a collapsible content section.
/// </summary>
[PseudoClasses(":expanded", ":up", ":down", ":left", ":right")]
public sealed class SiteBarExpander : HeaderedContentControl
{
        public static readonly StyledProperty<IPageTransition?> ContentTransitionProperty =
            AvaloniaProperty.Register<SiteBarExpander, IPageTransition?>(nameof(ContentTransition));

        public static readonly StyledProperty<ExpandDirection> ExpandDirectionProperty =
            AvaloniaProperty.Register<SiteBarExpander, ExpandDirection>(nameof(ExpandDirection));

        public static readonly DirectProperty<SiteBarExpander, bool> IsExpandedProperty =
            AvaloniaProperty.RegisterDirect<SiteBarExpander, bool>(
                nameof(IsExpanded),
                o => o.IsExpanded,
                (o, v) => o.IsExpanded = v,
                defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

        private bool _isExpanded;
        private CancellationTokenSource? _lastTransitionCts;

        static SiteBarExpander()
        {
            IsExpandedProperty.Changed.AddClassHandler<SiteBarExpander>((x, _) => x.OnIsExpandedChanged());
        }

        public SiteBarExpander()
        {
            UpdatePseudoClasses(ExpandDirection);
        }

        public IPageTransition? ContentTransition
        {
            get => GetValue(ContentTransitionProperty);
            set => SetValue(ContentTransitionProperty, value);
        }

        public ExpandDirection ExpandDirection
        {
            get => GetValue(ExpandDirectionProperty);
            set => SetValue(ExpandDirectionProperty, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set 
            { 
                SetAndRaise(IsExpandedProperty, ref _isExpanded, value);
                PseudoClasses.Set(":expanded", value);
            }
        }

        private async void OnIsExpandedChanged()
        {
            if (Content == null || ContentTransition == null || Presenter is not Visual visualContent)
            {
                return;
            }
            
            var forward = ExpandDirection is ExpandDirection.Left or ExpandDirection.Up;

            _lastTransitionCts?.Cancel();
            _lastTransitionCts = new CancellationTokenSource();

            if (IsExpanded)
            {
                await ContentTransition.Start(null, visualContent, forward, _lastTransitionCts.Token);
            }
            else
            {
                await ContentTransition.Start(visualContent, null, forward, _lastTransitionCts.Token);
            }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ExpandDirectionProperty)
            {
                UpdatePseudoClasses(change.NewValue.GetValueOrDefault<ExpandDirection>());
            }
        }

        private void UpdatePseudoClasses(ExpandDirection d)
        {
            PseudoClasses.Set(":up", d == ExpandDirection.Up);
            PseudoClasses.Set(":down", d == ExpandDirection.Down);
            PseudoClasses.Set(":left", d == ExpandDirection.Left);
            PseudoClasses.Set(":right", d == ExpandDirection.Right);
        }
}
