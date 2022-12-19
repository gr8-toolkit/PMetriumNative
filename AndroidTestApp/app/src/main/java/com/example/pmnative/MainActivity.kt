package com.example.pmnative

import android.os.Bundle
import android.util.Log
import android.view.View
import android.widget.Button
import android.widget.ImageView
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import com.bumptech.glide.Glide
import okhttp3.*
import okio.IOException
import java.util.concurrent.TimeUnit


class MainActivity : AppCompatActivity() {
    private val httpClient = OkHttpClient()

    lateinit var startButton: Button
    lateinit var stopButton: Button
    lateinit var resultCpuText: TextView

    lateinit var timeButton: Button
    lateinit var timeText: TextView

    lateinit var ramStartButton: Button
    lateinit var ramClearButton: Button
    lateinit var ramText: TextView

    lateinit var showGifButton: Button
    lateinit var hideGifButton: Button
    lateinit var gifImage: ImageView

    lateinit var eventsButton: Button


    val myCollection: MutableList<MyObject> = mutableListOf()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        startButton = findViewById(R.id.btStartCpu)
        stopButton = findViewById(R.id.btStopCpu)
        resultCpuText = findViewById(R.id.tvResultCpu)

        startButton.setOnClickListener {
            Log.i("PMETRIUM_NATIVE", "[START] CPU load " + System.currentTimeMillis())

            var result = 0

            for (i in 1..2500) {
                result = i

                for (i in 1..1000000) {
                    result = i
                }
            }

            resultCpuText.setText("Result: $result")

            Log.i("PMETRIUM_NATIVE", "[END] CPU load " + System.currentTimeMillis())
        }

        stopButton.setOnClickListener {
            resultCpuText.setText("Result: ")
        }

        timeButton = findViewById(R.id.btTime)
        timeText = findViewById(R.id.tvTime)

        timeButton.setOnClickListener {
            Log.i("PMETRIUM_NATIVE", "[START] Network activity " + System.currentTimeMillis())
            val requestImage = Request.Builder()
                .url("https://images.pexels.com/photos/807598/pexels-photo-807598.jpeg?cs=srgb&dl=pexels-sohail-nachiti-807598.jpg&fm=jpg&w=4128&h=3096")
                .build()

            for(i in 1..10){

                var status = false

                httpClient.newCall(requestImage).enqueue(object : Callback {
                    override fun onFailure(call: Call, e: IOException) {
                        e.printStackTrace()
                    }

                    override fun onResponse(call: Call, response: Response) {
                        response.use {
                            if (!response.isSuccessful) throw IOException("Unexpected code $response")

                            val responseStr = response.body?.string()
                            val pattern = "datetime\":\"(?<time>.*)\",\"day_of_week".toRegex()
                            val match = pattern.find(responseStr!!)
                            var result = match?.value
                            result = result?.replace("datetime\":\"", "")
                            result = result?.replace("\",\"day_of_week", "")

                            status = true
                        }
                    }
                })

                while(!status){
                    TimeUnit.MILLISECONDS.sleep(100)
                }
            }

            val request = Request.Builder()
                .url("https://worldtimeapi.org/api/timezone/Europe/Kiev")
                .build()

            var result = ""

            httpClient.newCall(request).enqueue(object : Callback {
                override fun onFailure(call: Call, e: IOException) {
                    e.printStackTrace()
                }

                override fun onResponse(call: Call, response: Response) {
                    response.use {
                        if (!response.isSuccessful) throw IOException("Unexpected code $response")

                        val responseStr = response.body?.string()
                        val pattern = "datetime\":\"(?<time>.*)\",\"day_of_week".toRegex()
                        val match = pattern.find(responseStr!!)
                        var result = match?.value
                        result = result?.replace("datetime\":\"", "")
                        result = result?.replace("\",\"day_of_week", "")

                        timeText.setText(result)
                    }
                }
            })

            Log.i("PMETRIUM_NATIVE", "[END] Network activity " + System.currentTimeMillis())
        }

        ramStartButton = findViewById(R.id.btRamStart)
        ramClearButton = findViewById(R.id.btRamClear)
        ramText = findViewById(R.id.tvRam)

        ramStartButton.setOnClickListener{
            Log.i("PMETRIUM_NATIVE", "RAM create usage " + System.currentTimeMillis())
            for(i in 1..1000000){
                myCollection.add(MyObject())
            }
            Log.i("PMETRIUM_NATIVE", "RAM created " + System.currentTimeMillis())
            ramText.setText("RAM: " + myCollection.size)
        }

        ramClearButton.setOnClickListener{
            myCollection.clear()
            Log.i("PMETRIUM_NATIVE", "RAM clear usage " + System.currentTimeMillis())
            ramText.setText("RAM: ")
        }

        showGifButton = findViewById(R.id.btShowGif)
        hideGifButton = findViewById(R.id.btHideGif)
        gifImage = findViewById(R.id.imGif)

        showGifButton.setOnClickListener {
            Log.i("PMETRIUM_NATIVE", "Show GIF " + System.currentTimeMillis())
            gifImage.setVisibility(View.VISIBLE);
            Glide.with(this)
                .load("https://i.gifer.com/origin/e8/e8896ab986acb162dd11ac07f87df885.gif")
                .into(gifImage)
        }

        hideGifButton.setOnClickListener {
            gifImage.setVisibility(View.GONE)
            Log.i("PMETRIUM_NATIVE", "Hide GIF " + System.currentTimeMillis())
        }

        eventsButton = findViewById(R.id.btEvents)

        eventsButton.setOnClickListener {
            Log.i("PMETRIUM_NATIVE", "[START] My test event " + System.currentTimeMillis())
            Thread.sleep(1)
            Log.i("PMETRIUM_NATIVE", "[START] My test event " + System.currentTimeMillis())
            Thread.sleep(1)
            Log.i("PMETRIUM_NATIVE", "[END] My test event " + System.currentTimeMillis())
            Thread.sleep(1)
            Log.i("PMETRIUM_NATIVE", "[START] My test event " + System.currentTimeMillis())
            Thread.sleep(1)
            Log.i("PMETRIUM_NATIVE", "[END] My test event " + System.currentTimeMillis())
            Thread.sleep(1)
            Log.i("PMETRIUM_NATIVE", "[END] My test event " + System.currentTimeMillis())
            Thread.sleep(1)

            Log.i("PMETRIUM_NATIVE", "[START] My test event " + System.currentTimeMillis())
            Thread.sleep(1)
            Log.i("PMETRIUM_NATIVE", "[END] My test event " + System.currentTimeMillis())
            Thread.sleep(1)

            Log.i("PMETRIUM_NATIVE", "[START] My test event " + System.currentTimeMillis())
            Thread.sleep(1)
            Log.i("PMETRIUM_NATIVE", "[START] My test event " + System.currentTimeMillis())
            Thread.sleep(1)
            Log.i("PMETRIUM_NATIVE", "[START] My test event " + System.currentTimeMillis())
            Thread.sleep(1)
            Log.i("PMETRIUM_NATIVE", "[START] Awesome event " + System.currentTimeMillis())
            Thread.sleep(1)
            Log.i("PMETRIUM_NATIVE", "[START] Awesome event " + System.currentTimeMillis())
            Thread.sleep(1)
            Log.i("PMETRIUM_NATIVE", "[END] Awesome event " + System.currentTimeMillis())
            Thread.sleep(1)
            Log.i("PMETRIUM_NATIVE", "[END] Awesome event " + System.currentTimeMillis())
            Thread.sleep(1)

            Log.i("PMETRIUM_NATIVE", "[START] Awesome event " + System.currentTimeMillis())
            Thread.sleep(1)
            Log.i("PMETRIUM_NATIVE", "[END] Awesome event " + System.currentTimeMillis())
            Thread.sleep(1)

            Log.i("PMETRIUM_NATIVE", "[END] My test event " + System.currentTimeMillis())
            Thread.sleep(1)
            Log.i("PMETRIUM_NATIVE", "[END] My test event " + System.currentTimeMillis())
        }
    }
}

class MyObject {
    public var Description: String = (System.currentTimeMillis()).toString()
}



