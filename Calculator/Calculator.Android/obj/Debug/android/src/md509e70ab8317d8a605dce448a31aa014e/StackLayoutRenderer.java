package md509e70ab8317d8a605dce448a31aa014e;


public class StackLayoutRenderer
	extends md51558244f76c53b6aeda52c8a337f2c37.VisualElementRenderer_1
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onInterceptTouchEvent:(Landroid/view/MotionEvent;)Z:GetOnInterceptTouchEvent_Landroid_view_MotionEvent_Handler\n" +
			"";
		mono.android.Runtime.register ("Calculator.Droid.StackLayoutRenderer, Calculator.Android", StackLayoutRenderer.class, __md_methods);
	}


	public StackLayoutRenderer (android.content.Context p0)
	{
		super (p0);
		if (getClass () == StackLayoutRenderer.class)
			mono.android.TypeManager.Activate ("Calculator.Droid.StackLayoutRenderer, Calculator.Android", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
	}


	public StackLayoutRenderer (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == StackLayoutRenderer.class)
			mono.android.TypeManager.Activate ("Calculator.Droid.StackLayoutRenderer, Calculator.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, mscorlib", this, new java.lang.Object[] { p0, p1, p2 });
	}


	public StackLayoutRenderer (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == StackLayoutRenderer.class)
			mono.android.TypeManager.Activate ("Calculator.Droid.StackLayoutRenderer, Calculator.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
	}


	public boolean onInterceptTouchEvent (android.view.MotionEvent p0)
	{
		return n_onInterceptTouchEvent (p0);
	}

	private native boolean n_onInterceptTouchEvent (android.view.MotionEvent p0);

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
