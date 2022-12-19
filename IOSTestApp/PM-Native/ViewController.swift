//
//  ViewController.swift
//  PM-Native
//
//  Created by Mykola Panasiuk on 19.09.2022.
//

import UIKit
import os

class ViewController: UIViewController {

    
    @IBOutlet weak var startButton: UIButton!
    @IBOutlet weak var textResult: UILabel!
    @IBOutlet weak var clearButton: UIButton!
    @IBOutlet weak var timeButton: UIButton!
    @IBOutlet weak var textTime: UILabel!
    @IBOutlet weak var imageTest: UIImageView!
    @IBOutlet weak var testGif: UIImageView!
    @IBOutlet weak var fps60Button: UIButton!
    @IBOutlet weak var hideGifs: UIButton!
    @IBOutlet weak var eventsButton: UIButton!
    
    var runStatus = true
    let defaultLog = Logger(subsystem: "PMETRIUM_NATIVE", category: "PMETRIUM_NATIVE")
    
    func unixTimestampUtc() -> Int64 {
        let utcDateFormatter = DateFormatter()
        utcDateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss.SSS'Z'"
        utcDateFormatter.timeZone = TimeZone(abbreviation: "UTC")
        
        let date = Date()
        let dateString = utcDateFormatter.string(from: date)
        let utcDate = utcDateFormatter.date(from: dateString)!
        
        return Int64(utcDate.timeIntervalSince1970 * 1000)
    }
    
    override func viewDidLoad() {
        super.viewDidLoad()
    
        
        testGif.isHidden = true
    }
    
    @IBAction func pressStart(_ sender: Any) {
        self.defaultLog.log("[PMETRIUM_NATIVE] [START] CPU load \(self.unixTimestampUtc())")

        var result = 0
        for index in 1...1000000 {
            result = index
            Thread.sleep(forTimeInterval: 0.000001)
        }
        
        self.textResult.text = "\(result)"
        
        self.defaultLog.log("[PMETRIUM_NATIVE] [END] CPU load \(self.unixTimestampUtc())")
    }
    
    
    @IBAction func clear(_ sender: Any) {
        textResult.text = ""
    }
    
    @IBAction func GenerateComplicatedEvents(_ sender: Any) {
        self.defaultLog.log("[PMETRIUM_NATIVE] [START] My test event \(self.unixTimestampUtc())")
        self.defaultLog.log("[PMETRIUM_NATIVE] [START] My test event \(self.unixTimestampUtc())")
 
        self.defaultLog.log("[PMETRIUM_NATIVE] [END] My test event \(self.unixTimestampUtc())")
        
        self.defaultLog.log("[PMETRIUM_NATIVE] [START] My test event \(self.unixTimestampUtc())")
        
        self.defaultLog.log("[PMETRIUM_NATIVE] [END] My test event \(self.unixTimestampUtc())")
        self.defaultLog.log("[PMETRIUM_NATIVE] [END] My test event \(self.unixTimestampUtc())")
        
        self.defaultLog.log("[PMETRIUM_NATIVE] [START] My test event \(self.unixTimestampUtc())")
        self.defaultLog.log("[PMETRIUM_NATIVE] [END] My test event \(self.unixTimestampUtc())")
        
        self.defaultLog.log("[PMETRIUM_NATIVE] [START] My test event \(self.unixTimestampUtc())")
        self.defaultLog.log("[PMETRIUM_NATIVE] [START] My test event \(self.unixTimestampUtc())")
        self.defaultLog.log("[PMETRIUM_NATIVE] [START] My test event \(self.unixTimestampUtc())")
        
        self.defaultLog.log("[PMETRIUM_NATIVE] [START] Awesome event \(self.unixTimestampUtc())")
        self.defaultLog.log("[PMETRIUM_NATIVE] [START] Awesome event \(self.unixTimestampUtc())")
        self.defaultLog.log("[PMETRIUM_NATIVE] [END] Awesome event \(self.unixTimestampUtc())")
        self.defaultLog.log("[PMETRIUM_NATIVE] [END] Awesome event \(self.unixTimestampUtc())")
        
        self.defaultLog.log("[PMETRIUM_NATIVE] [START] Awesome event \(self.unixTimestampUtc())")
        self.defaultLog.log("[PMETRIUM_NATIVE] [END] Awesome event \(self.unixTimestampUtc())")
 
        self.defaultLog.log("[PMETRIUM_NATIVE] [END] My test event \(self.unixTimestampUtc())")
        self.defaultLog.log("[PMETRIUM_NATIVE] [END] My test event \(self.unixTimestampUtc())")
    }
    
    @IBAction func getTime(_ sender: Any) {
        self.defaultLog.log("[PMETRIUM_NATIVE] [START] Network request \(self.unixTimestampUtc())")
        
        let urlLoad = URL(string: "https://images.pexels.com/photos/807598/pexels-photo-807598.jpeg?cs=srgb&dl=pexels-sohail-nachiti-807598.jpg&fm=jpg&w=4128&h=3096")!

        let semLoad = DispatchSemaphore(value: 0)
        
        let taskLoad = URLSession.shared.dataTask(with: urlLoad) {(data, response, error) in
                       
            semLoad.signal()
        }

        taskLoad.resume()
        semLoad.wait()
        
        
        let url = URL(string: "https://worldtimeapi.org/api/timezone/Europe/Kiev")!

        var myResult = ""
        let sem = DispatchSemaphore(value: 0)
        
        
        let task = URLSession.shared.dataTask(with: url) {(data, response, error) in
                       
            if let data = data, let dataString = String(data: data, encoding: .utf8) {
                print("Response data string:\n \(dataString)")
                
                let matched = self.matches(for: "datetime\":\"(?<time>.*)\",\"day_of_week", in: dataString)
                
                myResult = matched[0]
                    .replacingOccurrences(of: "datetime\":\"", with: "")
                    .replacingOccurrences(of: "\",\"day_of_week", with: "")
                
                print("====> " + myResult)
                
                
                
                sem.signal()
            }
        }

        task.resume()
        sem.wait()
    
        self.textTime.text = myResult
        
        self.defaultLog.log("[PMETRIUM_NATIVE] [END] Network request \(self.unixTimestampUtc())")
    }
    

    
    func matches(for regex: String, in text: String) -> [String] {

        do {
            let regex = try NSRegularExpression(pattern: regex)
            let results = regex.matches(in: text,
                                        range: NSRange(text.startIndex..., in: text))
            return results.map {
                String(text[Range($0.range, in: text)!])
            }
        } catch let error {
            print("invalid regex: \(error.localizedDescription)")
            return []
        }
    }
    
    
    @IBAction func ShowGif60(_ sender: Any) {
        self.defaultLog.log("[PMETRIUM_NATIVE] Show gif \(self.unixTimestampUtc())")
        
        let gif = UIImage.gifImageWithName("60fps")
        testGif.image = gif
        
        testGif.isHidden = false
    }
    
    @IBAction func HideGif(_ sender: Any) {
        self.defaultLog.log("[PMETRIUM_NATIVE] Hide gif \(self.unixTimestampUtc())")
        
        testGif.image = nil
        
        testGif.isHidden = true
    }
}

