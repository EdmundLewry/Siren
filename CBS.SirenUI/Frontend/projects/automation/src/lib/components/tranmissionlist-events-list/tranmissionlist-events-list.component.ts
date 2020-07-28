import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'lib-tranmissionlist-events-list',
  templateUrl: './tranmissionlist-events-list.component.html',
  styleUrls: ['./tranmissionlist-events-list.component.css']
})
export class TranmissionlistEventsListComponent implements OnInit {
  public readonly dataSource: MatTableDataSource<TransmissionListEvent> = new MatTableDataSource();

  public readonly displayedColumns: string[] = [
    "id",
    "eventState",
    "expectedDuration",
    "expectedStartTime"
  ]

  private readonly fakeData: TransmissionListEvent[] = [
    { id: 1, eventState: "Scheduled", expectedDuration: "00:00:30:00", expectedStartTime: "2020-03-22T00:00:10:05" },
    { id: 2, eventState: "Running", expectedDuration: "00:00:30:00", expectedStartTime: "2020-03-22T00:00:40:05" },
  ];

  constructor(private http: HttpClient) {
    this.dataSource.data = this.fakeData;
  }

  public ngOnInit() {
    this.http.get<TransmissionListEvent[]>('api/1/automation/transmissionlist/0').subscribe(result => {
      this.dataSource.data = result;
    }, error => console.error(error));
  }
}

interface TransmissionListEvent {
  id: number;
  eventState: string;
  expectedDuration: string;
  expectedStartTime: string;
}
