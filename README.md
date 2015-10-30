# QuizDemo

Quiz Demo is an Unity 5 mini-game used as proof of concept for the [unity-tracker](https://github.com/e-ucm/unity-tracker) module.

The [unity-tracker](https://github.com/e-ucm/unity-tracker) is used as a client that sends [xAPI statements](https://github.com/adlnet/xAPI-Spec/blob/master/xAPI.md#statement) to the RAGE analytics collector server.
The received statements are stored with the help of a [Learning Record Store](https://tincanapi.com/learning-record-store/) ([OpenLRS](https://github.com/e-ucm/OpenLRS)) in order to 
create some [xAPI dashboards](https://github.com/e-ucm/rage-analytics/wiki/OpenLRS-Dashboards).

The statements can also be processed by the [rage-realtime-module](https://github.com/e-ucm/rage-analytics-realtime) if a [class](https://github.com/e-ucm/rage-analytics/wiki/Set-up-a-class) is started by a teacher and
a [realtime-analytics model](https://github.com/e-ucm/rage-analytics/wiki/Realtime-analysis-model) is configured by a developer. This generates interesting [realtime dashboards](https://github.com/e-ucm/rage-analytics/wiki/Real-time-Dashboards) 
for the teacher.

A simple [realtime analytics model](https://github.com/e-ucm/rage-analytics/wiki/Realtime-analysis-model) 
for this Quiz Demo could look like this:

![1-config](https://cloud.githubusercontent.com/assets/5657407/10850092/17787994-7f24-11e5-8448-177ea43b1eeb.png)

### The Quiz Demo ###

The Demo consists of four simple screens.

The first screen propmts the player with a choice right before starting to play.

![2-start](https://cloud.githubusercontent.com/assets/5657407/10850095/177aab88-7f24-11e5-878f-53f10aaa54fb.png)

The second screen asks a multiple text-based choice question.

![3-q1](https://cloud.githubusercontent.com/assets/5657407/10850093/1778822c-7f24-11e5-999c-89f46e1030f5.png)

The third screen asks a similar question but this time using images instead of text as possible choices.

![4-q2](https://cloud.githubusercontent.com/assets/5657407/10850094/177a8568-7f24-11e5-8cc0-6446f22e8f9c.png)

The last screen showns the player its score.

![5-score](https://cloud.githubusercontent.com/assets/5657407/10850091/17520818-7f24-11e5-993b-f84aac6cb68b.png)

For each question correctly answered the score increses. Each time a question is answered wrong the score decreases. 
The maximum achievable score is 100.

For every interaction an xAPI statement is sent to the collector server and tracked by the [real-time module](https://github.com/e-ucm/rage-analytics-realtime) - if correctly configured.

There is also an upper console that logs all the statements that have been sent to the collector server.

### Configure the tracking code and the lost ###

The configuration process is very similar to the [Lost in Space](https://github.com/e-ucm/rage-analytics/wiki/Tracking-code#setting-up-the-tracking-code).

The [track.txt](https://github.com/e-ucm/QuizDemo/blob/master/Assets/Assets/track.txt) file must be edited changing the `host` and `trackingCode` 
with the correct values. Normaly the `host` looks like this `http://localhost:3000/api/proxy/gleaner/collector/` and the `trackingCode` normally looks like [this](https://github.com/e-ucm/rage-analytics/wiki/Tracking-code).
