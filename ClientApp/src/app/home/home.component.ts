import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, catchError } from 'rxjs/operators';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {
  //
  public events: Array<Event>;
  public inputs = new Inputs();
  public outputs = new Outputs();

  public lastEventIn = new Event();
  public lastEventOut = new Event();

  public pwm1 = new PWM(1);
  public pwm2 = new PWM(2);

  constructor(private httpClient: HttpClient) { }

  ngOnInit() {
    setInterval(() => this.getInputs(), 500);
    setInterval(() => this.listEvents(), 1000);
    setInterval(() => this.getLastEventIn(), 1000);
    setInterval(() => this.getLastEventOut(), 1000);
  }

  private getInputs() {
    this.httpClient
      .get('api/command/inputs')
      .pipe(catchError(() => 'INPUTS GET - ERROR'))
      .subscribe(
        (response: Inputs) => this.processInputs(response),
        (error: string) => console.log(error)
      );
  }

  private listEvents() {
    this.httpClient
      .get('api/command/event/in')
      .pipe(catchError(() => 'EVENT GET - ERROR'))
      .subscribe(
        (response: Array<Event>) => (this.events = response),
        (error: string) => console.log(error)
      );
  }

  private getLastEventIn() {
    this.httpClient
      .get('api/command/event/in/last')
      .pipe(catchError(() => 'LAST EVENT IN GET - ERROR'))
      .subscribe(
        (response: Event) => (this.lastEventIn = response),
        (error: string) => console.log(error)
      );
  }

  private getLastEventOut() {
    this.httpClient
      .get('api/command/event/out/last')
      .pipe(catchError(() => 'LAST EVENT OUT GET - ERROR'))
      .subscribe(
        (response: Event) => (this.lastEventOut = response),
        (error: string) => console.log(error)
      );
  }

  private processInputs(inputs: Inputs) {
    this.inputs = inputs;
    this.outputs.out1 = this.inputs.in1;
    this.outputs.out2 = this.inputs.in2;

    if (this.inputs.in1 || this.inputs.in2) {
      this.sendOutputs();
    }

    if (this.inputs.in3) {
      this.pwm1.addPower();
      this.sendPWM(this.pwm1);
    } else {
      this.pwm1.resetPower();
      this.sendPWM(this.pwm1);
    }

    if (this.inputs.in4) {
    }
  }

  private sendOutputs() {
    this.httpClient
      .post('api/command/outputs', this.outputs)
      .pipe(
        map(() => 'OUTPUTS POST - SUCCESS'),
        catchError(() => 'OUTPUTS POST - ERROR')
      )
      .subscribe(
        (success: string) => console.log(success),
        (error: string) => console.error(error)
      );
  }

  public sendPWM(pwm: PWM) {
    this.httpClient
      .post('api/command/pwm', pwm)
      .pipe(
        map(() => 'PWM POST - SUCCESS'),
        catchError(() => 'PWM POST - ERROR')
      )
      .subscribe(
        (success: string) => console.log(success),
        (error: string) => console.error(error)
      );
  }
}

export class Event {
  dataHoraEvento: string;
  nome: string;
  msg: string;
  cor: string;
}

export class Inputs {
  in1: number;
  in2: number;
  in3: number;
  in4: number;
  in5: number;

  constructor() {
    this.in1 = 0;
    this.in2 = 0;
    this.in3 = 0;
    this.in4 = 0;
    this.in5 = 0;
  }
}

export class Outputs {
  out1: number;
  out2: number;
  out3: number;
  out4: number;
  out5: number;

  constructor() {
    this.out1 = 0;
    this.out2 = 0;
    this.out3 = 0;
    this.out4 = 0;
    this.out5 = 0;
  }
}

export class PWM {
  canal: number;
  potencia: number;

  constructor(canal: number) {
    this.canal = canal;
    this.potencia = 0;
  }

  resetPower() {
    this.potencia = 0;
  }

  addPower() {
    if (this.potencia < 100) {
      this.potencia += 1;
    } else {
      this.resetPower();
    }
  }
}
