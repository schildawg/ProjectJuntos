function fib(n);
begin
    if n < 2 then exit n;
    exit fib(n - 1) + fib(n - 2);
end

print fib(7);