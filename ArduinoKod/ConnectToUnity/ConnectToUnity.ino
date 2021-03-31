//pins
int analogPin0 = 0;
int analogPin1 = 1;
int analogPin2 = 2;

//vars
int val = 0; // variable to store the value read
byte incomingByte;
String str;

void setup()
{
    Serial.begin(57600);

    while (!Serial);
    Serial.println("found serial");
}

void loop()
{
    str = "";
    while (Serial.available() > 0)
    {
        str = Serial.readStringUntil('\n');
    }

    if (str == "A")
    { //A
        val = analogRead(analogPin0);
        Serial.println(String("A") + val);
        Serial.flush();
    }
    else if (str == "B")
    { //B
        val = analogRead(analogPin1);
        Serial.println(String("B") + val);
        Serial.flush();
    }
    else if (str == "C")
    { //C
        val = analogRead(analogPin2);
        Serial.println(String("C") + val);
        Serial.flush();
    }
    else if (str == "GameIsland")
    { //H -> handshake
        Serial.println(String("GameIsland"));
        Serial.flush();
    }
}
