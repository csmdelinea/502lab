const { ajax, defer, fromEvent, interval, filter, map, catchError, of } = rxjs;


const url = '/api/diagnostics';

const getData = (url) => {
    return ajax.ajax.getJSON(url).pipe(
        map(response => response),
        catchError(error => {
            console.error('Error: ', error);
            return of(error); 
        })
    );
};


var viewModel = {}

function getDestiunations() {
    console.log('here');
}
getData(url).subscribe({
    next: data => {
        viewModel = ko.mapping.fromJS(data);
        ko.applyBindings(viewModel);
    },
    error: err => console.error('Error: ', err),
    complete: () => console.log('Request complete')
  });


