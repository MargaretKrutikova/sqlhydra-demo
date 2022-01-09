-- rambler up
SET
search_path TO demo_stuff,public;

create table thing
(
    id         text primary key,
    name       text        not null,
    owner      text        not null,
    age        int         not null,
    created_at timestamptz not null default current_timestamp
);

-- rambler down
SET
search_path TO demo_stuff,public;

DROP TABLE thing;
