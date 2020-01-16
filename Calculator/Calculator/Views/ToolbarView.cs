using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Extensions;

namespace Xamarin.Forms
{
    public class ToolbarView : FlexLayout, IEnumerable<View>
    {
        public ToolbarView()
        {
            Direction = FlexDirection.Row;
            Wrap = FlexWrap.NoWrap;
            JustifyContent = FlexJustify.SpaceBetween;
            AlignItems = FlexAlignItems.Center;
        }

        public void Add(View view) => Children.Add(view);

        public IEnumerator<View> GetEnumerator() => Children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
