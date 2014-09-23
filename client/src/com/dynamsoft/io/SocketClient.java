package com.dynamsoft.io;

import java.io.BufferedInputStream;
import java.io.BufferedOutputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.net.Socket;
import java.net.UnknownHostException;

import com.dynamsoft.ui.UIListener;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParseException;
import com.google.gson.JsonParser;

public class SocketClient extends Thread {
	private Socket mSocket;
	private UIListener mUIListener;
	
	public SocketClient(UIListener client) {
		mUIListener = client;
	}
	
	@Override
	public void run() {
		// TODO Auto-generated method stub
		super.run();
		
		try {
			mSocket = new Socket("192.168.8.84", 2015);
			BufferedOutputStream outputStream = new BufferedOutputStream(mSocket.getOutputStream());
			BufferedInputStream inputStream = new BufferedInputStream(mSocket.getInputStream());
			ByteArrayOutputStream out = new ByteArrayOutputStream();
			
			JsonObject jsonObj = new JsonObject();
            jsonObj.addProperty("type", "info");
            
			byte[] buff = new byte[256];
			int len = 0;
            String msg = null;
            outputStream.write(jsonObj.toString().getBytes());
            outputStream.flush();
            int sum = 0;
            int total = 0;    
            boolean isDataReady = false;
            
            while ((len = inputStream.read(buff)) != -1) {
            	if (!isDataReady) {
            		msg = new String(buff, 0, len);
                    
                    // JSON analysis
                    JsonParser parser = new JsonParser();
                    boolean isJSON = false;
                    JsonElement element = null;
                    try {
                        element =  parser.parse(msg);
                        
                        if (element != null) {
                        	isJSON = true;
                        }
                    }
                    catch (JsonParseException e) {
                        System.out.println("exception: " + e);
                    }
                    
                    if (isJSON) {
                    	System.out.println(element.toString());
                        JsonObject obj = element.getAsJsonObject();
                        element = obj.get("length");
                        if (element != null) {
                        	total = element.getAsInt();
                        	jsonObj = new JsonObject();
                            jsonObj.addProperty("type", "data");
                            outputStream.write(jsonObj.toString().getBytes());
                            outputStream.flush();
                            isDataReady = true;
                        }
                    }
            	}
                else {
                	out.write(buff, 0, len);  
                	sum += len;
                	if (sum == total) {
                		break;
                	}
                }
            }

            mUIListener.updateImage(out);
            System.out.println("close");
			outputStream.close();
			inputStream.close();
		} catch (UnknownHostException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		finally {
			try {
				mSocket.close();
				mSocket = null;
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
		
		System.out.println("data sent");
	}
	
	public void close() {
		if (mSocket != null) {
			try {
				mSocket.close();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
	}
}
