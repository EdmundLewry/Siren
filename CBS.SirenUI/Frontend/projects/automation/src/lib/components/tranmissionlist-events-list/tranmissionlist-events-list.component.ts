import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatTableDataSource } from '@angular/material/table';
import { ActivatedRoute } from '@angular/router';

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
    "expectedStartTime",
    "options"
  ];

  private itemId?: string;

  private readonly fakeData: TransmissionListEvent[] = [
    { id: 1, eventState: "Scheduled", expectedDuration: "00:00:30:00", expectedStartTime: "2020-03-22T00:00:10:05" },
    { id: 2, eventState: "Running", expectedDuration: "00:00:30:00", expectedStartTime: "2020-03-22T00:00:40:05" },
  ];

  constructor(private http: HttpClient,
              private route: ActivatedRoute) {
    this.dataSource.data = this.fakeData;
  }

  public ngOnInit() {
    if (this.route.snapshot.paramMap.has("itemId")) {
      this.itemId = this.route.snapshot.paramMap.get("itemId") as string;
    }

    this.http.get<TransmissionListEvent[]>(`/proxy/api/1/automation/transmissionlist/${this.itemId}/events`).subscribe(result => {
      //this.dataSource.data = result;
    }, error => console.error(error));
  }

  public requestDeleteConfirmation(listEvent: TransmissionListEvent): void {

  }
}


interface TransmissionListEvent {
  id: number;
  eventState: string;
  expectedDuration: string;
  expectedStartTime: string;
}
