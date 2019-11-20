/*
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
var showControllersOnly = false;
var seriesFilter = "";
var filtersOnlySampleSeries = true;

/*
 * Add header in statistics table to group metrics by category
 * format
 *
 */
function summaryTableHeader(header) {
    var newRow = header.insertRow(-1);
    newRow.className = "tablesorter-no-sort";
    var cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 1;
    cell.innerHTML = "Requests";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 3;
    cell.innerHTML = "Executions";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 6;
    cell.innerHTML = "Response Times (ms)";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 1;
    cell.innerHTML = "Throughput";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 2;
    cell.innerHTML = "Network (KB/sec)";
    newRow.appendChild(cell);
}

/*
 * Populates the table identified by id parameter with the specified data and
 * format
 *
 */
function createTable(table, info, formatter, defaultSorts, seriesIndex, headerCreator) {
    var tableRef = table[0];

    // Create header and populate it with data.titles array
    var header = tableRef.createTHead();

    // Call callback is available
    if(headerCreator) {
        headerCreator(header);
    }

    var newRow = header.insertRow(-1);
    for (var index = 0; index < info.titles.length; index++) {
        var cell = document.createElement('th');
        cell.innerHTML = info.titles[index];
        newRow.appendChild(cell);
    }

    var tBody;

    // Create overall body if defined
    if(info.overall){
        tBody = document.createElement('tbody');
        tBody.className = "tablesorter-no-sort";
        tableRef.appendChild(tBody);
        var newRow = tBody.insertRow(-1);
        var data = info.overall.data;
        for(var index=0;index < data.length; index++){
            var cell = newRow.insertCell(-1);
            cell.innerHTML = formatter ? formatter(index, data[index]): data[index];
        }
    }

    // Create regular body
    tBody = document.createElement('tbody');
    tableRef.appendChild(tBody);

    var regexp;
    if(seriesFilter) {
        regexp = new RegExp(seriesFilter, 'i');
    }
    // Populate body with data.items array
    for(var index=0; index < info.items.length; index++){
        var item = info.items[index];
        if((!regexp || filtersOnlySampleSeries && !info.supportsControllersDiscrimination || regexp.test(item.data[seriesIndex]))
                &&
                (!showControllersOnly || !info.supportsControllersDiscrimination || item.isController)){
            if(item.data.length > 0) {
                var newRow = tBody.insertRow(-1);
                for(var col=0; col < item.data.length; col++){
                    var cell = newRow.insertCell(-1);
                    cell.innerHTML = formatter ? formatter(col, item.data[col]) : item.data[col];
                }
            }
        }
    }

    // Add support of columns sort
    table.tablesorter({sortList : defaultSorts});
}

$(document).ready(function() {

    // Customize table sorter default options
    $.extend( $.tablesorter.defaults, {
        theme: 'blue',
        cssInfoBlock: "tablesorter-no-sort",
        widthFixed: true,
        widgets: ['zebra']
    });

    var data = {"OkPercent": 74.33333333333333, "KoPercent": 25.666666666666668};
    var dataset = [
        {
            "label" : "KO",
            "data" : data.KoPercent,
            "color" : "#FF6347"
        },
        {
            "label" : "OK",
            "data" : data.OkPercent,
            "color" : "#9ACD32"
        }];
    $.plot($("#flot-requests-summary"), dataset, {
        series : {
            pie : {
                show : true,
                radius : 1,
                label : {
                    show : true,
                    radius : 3 / 4,
                    formatter : function(label, series) {
                        return '<div style="font-size:8pt;text-align:center;padding:2px;color:white;">'
                            + label
                            + '<br/>'
                            + Math.round10(series.percent, -2)
                            + '%</div>';
                    },
                    background : {
                        opacity : 0.5,
                        color : '#000'
                    }
                }
            }
        },
        legend : {
            show : true
        }
    });

    // Creates APDEX table
    createTable($("#apdexTable"), {"supportsControllersDiscrimination": true, "overall": {"data": [0.05333333333333334, 500, 1500, "Total"], "isController": false}, "titles": ["Apdex", "T (Toleration threshold)", "F (Frustration threshold)", "Label"], "items": [{"data": [0.03, 500, 1500, "Bot Service: Message Blind Spots"], "isController": false}, {"data": [0.0, 500, 1500, "Bot Service: Message Attractor"], "isController": false}, {"data": [0.0, 500, 1500, "Bot Service: Conversation Update"], "isController": false}, {"data": [0.04, 500, 1500, "Bot Service: Message search Ongs Hat"], "isController": false}, {"data": [0.22, 500, 1500, "Bot Service: Message No"], "isController": false}]}, function(index, item){
        switch(index){
            case 0:
                item = item.toFixed(3);
                break;
            case 1:
            case 2:
                item = formatDuration(item);
                break;
        }
        return item;
    }, [[0, 0]], 3);

    // Create statistics table
    createTable($("#statisticsTable"), {"supportsControllersDiscrimination": true, "overall": {"data": ["Total", 300, 77, 25.666666666666668, 20527.853333333325, 21, 309923, 10527.800000000003, 301970.1, 308610.93, 0.901648212933242, 0.7975477610572126, 0.0], "isController": false}, "titles": ["Label", "#Samples", "KO", "Error %", "Average", "Min", "Max", "90th pct", "95th pct", "99th pct", "Transactions\/s", "Received", "Sent"], "items": [{"data": ["Bot Service: Message Blind Spots", 100, 22, 22.0, 53748.06999999999, 82, 309923, 304149.4, 306889.1, 309922.13, 0.30856292986673167, 0.2312926250759836, 0.0], "isController": false}, {"data": ["Bot Service: Message Attractor", 50, 50, 100.0, 2205.96, 21, 8661, 4696.5, 5095.549999999997, 8661.0, 0.15929248649199715, 0.02345519639967122, 0.0], "isController": false}, {"data": ["Bot Service: Conversation Update", 50, 0, 0.0, 4541.620000000001, 3009, 11279, 7022.599999999999, 9815.05, 11279.0, 4.1356492969396195, 2.3981111714226633, 0.0], "isController": false}, {"data": ["Bot Service: Message search Ongs Hat", 50, 0, 0.0, 5310.96, 1052, 19649, 9637.699999999999, 12466.099999999993, 19649.0, 2.1162229652516187, 4.910505261459347, 0.0], "isController": false}, {"data": ["Bot Service: Message No", 50, 5, 10.0, 3612.4399999999996, 25, 11272, 9023.9, 10013.949999999997, 11272.0, 0.15746494043101306, 0.1197656193411037, 0.0], "isController": false}]}, function(index, item){
        switch(index){
            // Errors pct
            case 3:
                item = item.toFixed(2) + '%';
                break;
            // Mean
            case 4:
            // Mean
            case 7:
            // Percentile 1
            case 8:
            // Percentile 2
            case 9:
            // Percentile 3
            case 10:
            // Throughput
            case 11:
            // Kbytes/s
            case 12:
            // Sent Kbytes/s
                item = item.toFixed(2);
                break;
        }
        return item;
    }, [[0, 0]], 0, summaryTableHeader);

    // Create error table
    createTable($("#errorsTable"), {"supportsControllersDiscrimination": false, "titles": ["Type of error", "Number of errors", "% in errors", "% in all samples"], "items": [{"data": ["Test failed: text expected to contain \\\/Choose the kind of blind spot:\\\/", 6, 7.792207792207792, 2.0], "isController": false}, {"data": ["Test failed: text expected to contain \\\/Wait a minute. It will take a while.\\\/", 50, 64.93506493506493, 16.666666666666668], "isController": false}, {"data": ["Test failed: text expected to contain \\\/Random Point generated\\\/", 16, 20.77922077922078, 5.333333333333333], "isController": false}, {"data": ["Test failed: text expected to contain \\\/What would you like to get\\\/check?\\\/", 5, 6.4935064935064934, 1.6666666666666667], "isController": false}]}, function(index, item){
        switch(index){
            case 2:
            case 3:
                item = item.toFixed(2) + '%';
                break;
        }
        return item;
    }, [[1, 1]]);

        // Create top5 errors by sampler
    createTable($("#top5ErrorsBySamplerTable"), {"supportsControllersDiscrimination": false, "overall": {"data": ["Total", 300, 77, "Test failed: text expected to contain \\\/Wait a minute. It will take a while.\\\/", 50, "Test failed: text expected to contain \\\/Random Point generated\\\/", 16, "Test failed: text expected to contain \\\/Choose the kind of blind spot:\\\/", 6, "Test failed: text expected to contain \\\/What would you like to get\\\/check?\\\/", 5, null, null], "isController": false}, "titles": ["Sample", "#Samples", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors"], "items": [{"data": ["Bot Service: Message Blind Spots", 100, 22, "Test failed: text expected to contain \\\/Random Point generated\\\/", 16, "Test failed: text expected to contain \\\/Choose the kind of blind spot:\\\/", 6, null, null, null, null, null, null], "isController": false}, {"data": ["Bot Service: Message Attractor", 50, 50, "Test failed: text expected to contain \\\/Wait a minute. It will take a while.\\\/", 50, null, null, null, null, null, null, null, null], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": ["Bot Service: Message No", 50, 5, "Test failed: text expected to contain \\\/What would you like to get\\\/check?\\\/", 5, null, null, null, null, null, null, null, null], "isController": false}]}, function(index, item){
        return item;
    }, [[0, 0]], 0);

});
