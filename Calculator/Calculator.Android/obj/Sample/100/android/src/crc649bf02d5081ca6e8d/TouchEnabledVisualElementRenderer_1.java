package crc649bf02d5081ca6e8d;


public class TouchEnabledVisualElementRenderer_1
	extends crc643f46942d9dd1fff9.VisualElementRenderer_1
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_dispatchTouchEvent:(Landroid/view/MotionEvent;)Z:GetDispatchTouchEvent_Landroid_view_MotionEvent_Handler\n" +
			"n_onInterceptTouchEvent:(Landroid/view/MotionEvent;)Z:GetOnInterceptTouchEvent_Landroid_view_MotionEvent_Handler\n" +
			"n_onTouchEvent:(Landroid/view/MotionEvent;)Z:GetOnTouchEvent_Landroid_view_MotionEvent_Handler\n" +
			"";
		mono.android.Runtime.register ("Calculator.Droid.TouchEnabledVisualElementRenderer`1, Calculator.Android", TouchEnabledVisualElementRenderer_1.class, __md_methods);
	}


	public TouchEnabledVisualElementRenderer_1 (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == TouchEnabledVisualElementRenderer_1.class)
			mono.android.TypeManager.Activate ("Calculator.Droid.TouchEnabledVisualElementRenderer`1, Calculator.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, mscorlib", this, new java.lang.Object[] { p0, p1, p2 });
	}


	public TouchEnabledVisualElementRenderer_1 (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == TouchEnabledVisualElementRenderer_1.class)
			mono.android.TypeManager.Activate ("Calculator.Droid.TouchEnabledVisualElementRenderer`1, Calculator.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
	}


	public TouchEnabledVisualElementRenderer_1 (android.content.Context p0)
	{
		super (p0);
		if (getClass () == TouchEnabledVisualElementRenderer_1.class)
			mono.android.TypeManager.Activate ("Calculator.Droid.TouchEnabledVisualElementRenderer`1, Calculator.Android", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
	}


	public boolean dispatchTouchEvent (android.view.MotionEvent p0)
	{
		return n_dispatchTouchEvent (p0);
	}

	private native boolean n_dispatchTouchEvent (android.view.MotionEvent p0);


	public boolean onInterceptTouchEvent (android.view.MotionEvent p0)
	{
		return n_onInterceptTouchEvent (p0);
	}

	private native boolean n_onInterceptTouchEvent (android.view.MotionEvent p0);


	public boolean onTouchEvent (android.view.MotionEvent p0)
	{
		return n_onTouchEvent (p0);
	}

	private native boolean n_onTouchEvent (android.view.MotionEvent p0);

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
