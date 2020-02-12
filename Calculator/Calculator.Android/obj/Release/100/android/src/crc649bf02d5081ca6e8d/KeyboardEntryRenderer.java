package crc649bf02d5081ca6e8d;


public class KeyboardEntryRenderer
	extends crc643f46942d9dd1fff9.EntryRenderer
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_clearChildFocus:(Landroid/view/View;)V:GetClearChildFocus_Landroid_view_View_Handler\n" +
			"n_isFocused:()Z:GetIsFocusedHandler\n" +
			"n_clearFocus:()V:GetClearFocusHandler\n" +
			"n_onKeyDown:(ILandroid/view/KeyEvent;)Z:GetOnKeyDown_ILandroid_view_KeyEvent_Handler\n" +
			"";
		mono.android.Runtime.register ("Calculator.Droid.KeyboardEntryRenderer, Calculator.Android", KeyboardEntryRenderer.class, __md_methods);
	}


	public KeyboardEntryRenderer (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == KeyboardEntryRenderer.class)
			mono.android.TypeManager.Activate ("Calculator.Droid.KeyboardEntryRenderer, Calculator.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, mscorlib", this, new java.lang.Object[] { p0, p1, p2 });
	}


	public KeyboardEntryRenderer (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == KeyboardEntryRenderer.class)
			mono.android.TypeManager.Activate ("Calculator.Droid.KeyboardEntryRenderer, Calculator.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
	}


	public KeyboardEntryRenderer (android.content.Context p0)
	{
		super (p0);
		if (getClass () == KeyboardEntryRenderer.class)
			mono.android.TypeManager.Activate ("Calculator.Droid.KeyboardEntryRenderer, Calculator.Android", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
	}


	public void clearChildFocus (android.view.View p0)
	{
		n_clearChildFocus (p0);
	}

	private native void n_clearChildFocus (android.view.View p0);


	public boolean isFocused ()
	{
		return n_isFocused ();
	}

	private native boolean n_isFocused ();


	public void clearFocus ()
	{
		n_clearFocus ();
	}

	private native void n_clearFocus ();


	public boolean onKeyDown (int p0, android.view.KeyEvent p1)
	{
		return n_onKeyDown (p0, p1);
	}

	private native boolean n_onKeyDown (int p0, android.view.KeyEvent p1);

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
