// David Qiu (david@davidqiu.com)
// 2014.6.11
// Stepper controller program

#include <Arduino.h>

#define A1 2
#define B1 3
#define C1 4
#define D1 5
#define DL 5

// 512 = 360 degrees
#define ST 512


void Phase_A()
{
  digitalWrite(A1, HIGH); // Pin A1 in HIGH
  digitalWrite(B1, LOW);
  digitalWrite(C1, LOW);
  digitalWrite(D1, LOW);
}

void Phase_B()
{
  digitalWrite(A1, LOW);
  digitalWrite(B1, HIGH); // Pin B1 in HIGH
  digitalWrite(C1, LOW);
  digitalWrite(D1, LOW);
}

void Phase_C()
{
  digitalWrite(A1, LOW);
  digitalWrite(B1, LOW);
  digitalWrite(C1, HIGH); // Pin C1 in HIGH
  digitalWrite(D1, LOW);
}

void Phase_D()
{
  digitalWrite(A1, LOW);
  digitalWrite(B1, LOW);
  digitalWrite(C1, LOW);
  digitalWrite(D1, HIGH); // Pin D1 in HIGH
}


void setup()
{
  // Initialize the serial port
  Serial.begin(57600);

  // Initialize the output pins
  pinMode(A1, OUTPUT);
  pinMode(B1, OUTPUT);
  pinMode(C1, OUTPUT);
  pinMode(D1, OUTPUT);

  // Send initialization finish signal
  Serial.print("$$INIT$$\n");
}

void loop()
{
  // Read rotation command: "[CMD]"
  int cmdAngle = Serial.parseInt();
  if (cmdAngle <= 0 || cmdAngle>512) return;
  Serial.flush();

  // Send rotation command to confirm: "$$[CMD]$$"
  Serial.print("$$:"); Serial.print(cmdAngle); Serial.print("$$\n");

  // Check confirm: "OK"
  char ch_read;
  while (ch_read = Serial.read(), ch_read == -1);
  if (ch_read != 'O')
  {
    Serial.print("$$RESET$$\n");
    return;
  }
  while (ch_read = Serial.read(), ch_read == -1);
  if (ch_read != 'K')
  {
    Serial.print("$$RESET$$\n");
    return;
  }

  // Start rotatation
  Serial.print("$$START$$\n");
  int i;
  for (i = 0; i<cmdAngle; ++i)
  {
    // Set the phase of A pin and delay
    Phase_A();
    delay(DL);

    // Set the phase of B pin and delay
    Phase_B();
    delay(DL);

    // Set the phase of C pin and delay
    Phase_C();
    delay(DL);

    // Set the phase of D pin and delay
    Phase_D();
    delay(DL);
  }

  // Send success signal
  Serial.print("$$FINISH$$\n");
}
