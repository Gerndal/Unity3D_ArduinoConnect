#include <Arduino.h>

int ledPin[3] = {3, 5, 6};
int buttonPin[3] = {9, 10, 11};
String retStr[3] = {"A", "B", "C"};
bool idDown[3] = {false, false, false};

void IO_Initialize();
void Connection();
void Handshake();
void ButtonCheck(int _button);

void setup()
{
    IO_Initialize();
    Connection();
}

void loop()
{
    Handshake();

    for (int i = 0; i < 3; ++i) ButtonCheck(i);
    delay(2);
}

void ButtonCheck(int _btnIdx)
{
    int idex = buttonPin[_btnIdx] - buttonPin[0];
    if (digitalRead(buttonPin[_btnIdx]) == LOW)
    {
        if (idDown[idex] == true) return;

        digitalWrite(ledPin[idex], HIGH);
        Serial.println(retStr[idex]);
        idDown[idex] = true;
    }
    else
    {
        if (idDown[idex] == false) return;

        digitalWrite(ledPin[idex], LOW);
        Serial.println("");
        idDown[idex] = false;
    }
}

void Handshake()
{
    String str = "";
    while (Serial.available() > 0)
    {
        str = Serial.readStringUntil('\n');
    }

    if (str == "GameIsland")
    {
        Serial.println(String("GameIsland"));
    }
}

void Connection()
{
    Serial.begin(57600);

    while (!Serial);
    Serial.println("Serial Connected");
}

void IO_Initialize()
{
    for (int i = 0; i < 3; ++i)
    {
        pinMode(ledPin[i], OUTPUT);
        pinMode(buttonPin[i], INPUT_PULLUP);
    }
}

