var nums = new[] { 1, 2, 3, 4, 5 };

// filter
var evens = nums.Where(x => x % 2 == 0);

// select
var squares = nums.Select(n => n * n);

// sort
var sorted = nums.OrderByDescending(n => n);

//aggregates
int sum = nums.Sum();
double avg = nums.Average();
bool anyEven = nums.Any(n => n % 2 == 0);


// group 
var grouped = nums.GroupBy(x => x % 2 == 0 ? "even" : "odd");

var query = nums.Where(x => x % 2 == 0).Select(x => x);
var querySame = from num in nums
                where num > 2
                orderby num
                select num * 2;

