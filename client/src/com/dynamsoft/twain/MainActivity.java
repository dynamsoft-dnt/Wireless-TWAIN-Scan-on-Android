package com.dynamsoft.twain;

import java.io.ByteArrayOutputStream;

import android.app.Activity;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Bundle;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.ImageView;

import com.dynamsoft.io.SocketClient;
import com.dynamsoft.ui.UIListener;

public class MainActivity extends Activity implements UIListener, OnClickListener{
	private Button mButton;
	private ImageView mImageView;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		
		mButton  = (Button)findViewById(R.id.scan);
		mButton.setOnClickListener(this);
		
		mImageView = (ImageView)findViewById(R.id.image);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}

	@Override
	public void updateImage(ByteArrayOutputStream out) {
		byte[] bytes = out.toByteArray();
		final Bitmap bitmap = BitmapFactory.decodeByteArray(bytes, 0, bytes.length);
		// TODO Auto-generated method stub
		runOnUiThread(new Runnable(){

			@Override
			public void run() {
				// TODO Auto-generated method stub
				
				mImageView.setImageBitmap(bitmap);
			}
			
		});
	}

	@Override
	public void onClick(View v) {
		// TODO Auto-generated method stub
		
		SocketClient client = new SocketClient(this);
		client.start();
	}

}
