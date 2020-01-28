package crc649bf02d5081ca6e8d;


public class DrawerListViewRenderer
	extends crc643f46942d9dd1fff9.ListViewRenderer
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_dispatchTouchEvent:(Landroid/view/MotionEvent;)Z:GetDispatchTouchEvent_Landroid_view_MotionEvent_Handler\n" +
			"n_overScrollBy:(IIIIIIIIZ)Z:GetOverScrollBy_IIIIIIIIZHandler\n" +
			"n_onOverScrolled:(IIZZ)V:GetOnOverScrolled_IIZZHandler\n" +
			"n_initializeScrollbars:(Landroid/content/res/TypedArray;)V:GetInitializeScrollbars_Landroid_content_res_TypedArray_Handler\n" +
			"n_computeVerticalScrollOffset:()I:GetComputeVerticalScrollOffsetHandler\n" +
			"n_awakenScrollBars:()Z:GetAwakenScrollBarsHandler\n" +
			"n_computeVerticalScrollExtent:()I:GetComputeVerticalScrollExtentHandler\n" +
			"n_computeVerticalScrollRange:()I:GetComputeVerticalScrollRangeHandler\n" +
			"n_scrollTo:(II)V:GetScrollTo_IIHandler\n" +
			"n_onScrollChanged:(IIII)V:GetOnScrollChanged_IIIIHandler\n" +
			"";
		mono.android.Runtime.register ("Calculator.Droid.DrawerListViewRenderer, Calculator.Android", DrawerListViewRenderer.class, __md_methods);
	}


	public DrawerListViewRenderer (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == DrawerListViewRenderer.class)
			mono.android.TypeManager.Activate ("Calculator.Droid.DrawerListViewRenderer, Calculator.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, mscorlib", this, new java.lang.Object[] { p0, p1, p2 });
	}


	public DrawerListViewRenderer (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == DrawerListViewRenderer.class)
			mono.android.TypeManager.Activate ("Calculator.Droid.DrawerListViewRenderer, Calculator.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
	}


	public DrawerListViewRenderer (android.content.Context p0)
	{
		super (p0);
		if (getClass () == DrawerListViewRenderer.class)
			mono.android.TypeManager.Activate ("Calculator.Droid.DrawerListViewRenderer, Calculator.Android", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
	}


	public boolean dispatchTouchEvent (android.view.MotionEvent p0)
	{
		return n_dispatchTouchEvent (p0);
	}

	private native boolean n_dispatchTouchEvent (android.view.MotionEvent p0);


	public boolean overScrollBy (int p0, int p1, int p2, int p3, int p4, int p5, int p6, int p7, boolean p8)
	{
		return n_overScrollBy (p0, p1, p2, p3, p4, p5, p6, p7, p8);
	}

	private native boolean n_overScrollBy (int p0, int p1, int p2, int p3, int p4, int p5, int p6, int p7, boolean p8);


	public void onOverScrolled (int p0, int p1, boolean p2, boolean p3)
	{
		n_onOverScrolled (p0, p1, p2, p3);
	}

	private native void n_onOverScrolled (int p0, int p1, boolean p2, boolean p3);


	public void initializeScrollbars (android.content.res.TypedArray p0)
	{
		n_initializeScrollbars (p0);
	}

	private native void n_initializeScrollbars (android.content.res.TypedArray p0);


	public int computeVerticalScrollOffset ()
	{
		return n_computeVerticalScrollOffset ();
	}

	private native int n_computeVerticalScrollOffset ();


	public boolean awakenScrollBars ()
	{
		return n_awakenScrollBars ();
	}

	private native boolean n_awakenScrollBars ();


	public int computeVerticalScrollExtent ()
	{
		return n_computeVerticalScrollExtent ();
	}

	private native int n_computeVerticalScrollExtent ();


	public int computeVerticalScrollRange ()
	{
		return n_computeVerticalScrollRange ();
	}

	private native int n_computeVerticalScrollRange ();


	public void scrollTo (int p0, int p1)
	{
		n_scrollTo (p0, p1);
	}

	private native void n_scrollTo (int p0, int p1);


	public void onScrollChanged (int p0, int p1, int p2, int p3)
	{
		n_onScrollChanged (p0, p1, p2, p3);
	}

	private native void n_onScrollChanged (int p0, int p1, int p2, int p3);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
