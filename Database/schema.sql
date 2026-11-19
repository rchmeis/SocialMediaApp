
-- =========================================================
-- 1. DROP EXISTING TABLES IF EXISTS
-- Drop dependent tables before referenced tables.
-- =========================================================

DROP TABLE IF EXISTS `user_attends_event`;
DROP TABLE IF EXISTS `event_images`;
DROP TABLE IF EXISTS `post_images`;
DROP TABLE IF EXISTS `user_likes_comment`;
DROP TABLE IF EXISTS `user_likes_post`;
DROP TABLE IF EXISTS `user_interests`;
DROP TABLE IF EXISTS `institute_courses`;
DROP TABLE IF EXISTS `user_blocks_user`;
DROP TABLE IF EXISTS `user_follows_user`;
DROP TABLE IF EXISTS `student_jobs`;
DROP TABLE IF EXISTS `private_messages`;
DROP TABLE IF EXISTS `events`;
DROP TABLE IF EXISTS `comments`;
DROP TABLE IF EXISTS `posts`;
DROP TABLE IF EXISTS `students`;
DROP TABLE IF EXISTS `partners`;
DROP TABLE IF EXISTS `institutes`;
DROP TABLE IF EXISTS `courses`;
DROP TABLE IF EXISTS `users`;
DROP TABLE IF EXISTS `images`;
DROP TABLE IF EXISTS `interests`;
DROP TABLE IF EXISTS `cities`;
DROP TABLE IF EXISTS `fields_of_study`;
DROP TABLE IF EXISTS `user_authentications`;


-- =========================================================
-- 2. INDEPENDENT TABLES
-- These tables don't reference other application tables.
-- =========================================================

-- user_authentications
CREATE TABLE `user_authentications` (
    `user_authentication_id` INT                                              NOT NULL AUTO_INCREMENT,
    `username`               VARCHAR(20)                                      NOT NULL UNIQUE,
    `email`                  VARCHAR(255)                                     NOT NULL UNIQUE,
    `password_hash`          VARCHAR(255)                                     NOT NULL,
    `is_verified`            BOOLEAN                                          NOT NULL DEFAULT FALSE,
    `role`                   ENUM('Student', 'Admin', 'Moderator', 'Partner') NOT NULL DEFAULT 'Student',

    PRIMARY KEY (`user_authentication_id`)
);

-- images
CREATE TABLE `images` (
    `image_id`    INT          NOT NULL AUTO_INCREMENT,
    `file_name`   VARCHAR(255) NOT NULL,
    `alt_text`    VARCHAR(255) NOT NULL DEFAULT '',
    `image_data`  BLOB         NOT NULL,
    `height`      INT          NOT NULL,
    `width`       INT          NOT NULL,
    `file_size`   INT          NOT NULL,
    `created_at`  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`image_id`)
);

-- cities
CREATE TABLE `cities` (
    `city_id`     INT          NOT NULL AUTO_INCREMENT,
    `name`        VARCHAR(127) NOT NULL,
    `postal_code` CHAR(4)      NOT NULL,

    PRIMARY KEY (`city_id`)
);

-- fields_of_study
CREATE TABLE `fields_of_study` (
    `field_of_study_id` INT          NOT NULL AUTO_INCREMENT,
    `name`              VARCHAR(127) NOT NULL,

    PRIMARY KEY (`field_of_study_id`)
);

-- interests
CREATE TABLE `interests` (
    `interest_id` INT          NOT NULL AUTO_INCREMENT,
    `name`        VARCHAR(127) NOT NULL,

    PRIMARY KEY (`interest_id`)
);


-- =========================================================
-- 3. CORE TABLES
-- =========================================================

-- users
CREATE TABLE `users` (
    `user_id`      INT          NOT NULL AUTO_INCREMENT,
    `user_auth_id` INT          NOT NULL UNIQUE,
    `bio`          VARCHAR(255) NOT NULL DEFAULT '',
    `image_id`     INT          NULL,
    `created_at`   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `deleted_at`   DATETIME     NULL,

    PRIMARY KEY (`user_id`),

    FOREIGN KEY (`user_auth_id`)
        REFERENCES `user_authentications`(`user_authentication_id`)
        ON DELETE CASCADE,

    FOREIGN KEY (`image_id`)
        REFERENCES `images`(`image_id`)
        ON DELETE SET NULL
);

-- institutes
CREATE TABLE `institutes` (
    `institute_id` INT          NOT NULL AUTO_INCREMENT,
    `name`         VARCHAR(127) NOT NULL,
    `domain_name`  VARCHAR(63)  NOT NULL UNIQUE,
    `city_id`      INT          NOT NULL,

    PRIMARY KEY (`institute_id`),

    FOREIGN KEY (`city_id`)
        REFERENCES `cities`(`city_id`)
);

-- courses
CREATE TABLE `courses` (
    `course_id`        INT          NOT NULL AUTO_INCREMENT,
    `name`             VARCHAR(127) NOT NULL,
    `field_of_study_id` INT         NOT NULL,

    PRIMARY KEY (`course_id`),

    FOREIGN KEY (`field_of_study_id`)
        REFERENCES `fields_of_study`(`field_of_study_id`)
        ON DELETE CASCADE
);


-- =========================================================
-- 4. INSTITUTE-COURSE RELATIONSHIP
-- =========================================================

-- institute_courses
CREATE TABLE `institute_courses` (
    `institute_id` INT NOT NULL,
    `course_id`    INT NOT NULL,

    PRIMARY KEY (`institute_id`, `course_id`),

    FOREIGN KEY (`institute_id`)
        REFERENCES `institutes`(`institute_id`)
        ON DELETE CASCADE,

    FOREIGN KEY (`course_id`)
        REFERENCES `courses`(`course_id`)
        ON DELETE CASCADE
);


-- =========================================================
-- 5. USER SPECIALIZATION TABLES
-- =========================================================

-- students
CREATE TABLE `students` (
    `user_id`      INT         NOT NULL,
    `first_name`   VARCHAR(63) NOT NULL,
    `last_name`    VARCHAR(63) NOT NULL,
    `is_onboarded` BOOLEAN     NOT NULL DEFAULT FALSE,
    `institute_id` INT         NOT NULL,
    `course_id`    INT         NOT NULL,

    PRIMARY KEY (`user_id`),

    FOREIGN KEY (`user_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE,

    FOREIGN KEY (`institute_id`)
        REFERENCES `institutes`(`institute_id`),

    FOREIGN KEY (`course_id`)
        REFERENCES `courses`(`course_id`)
);

-- partners
CREATE TABLE `partners` (
    `user_id`              INT          NOT NULL,
    `name_of_organization` VARCHAR(127) NOT NULL,

    PRIMARY KEY (`user_id`),

    FOREIGN KEY (`user_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE
);


-- =========================================================
-- 6. CONTENT TABLES
-- =========================================================

-- posts
CREATE TABLE `posts` (
    `post_id`    INT         NOT NULL AUTO_INCREMENT,
    `author_id`  INT         NOT NULL,
    `title`      VARCHAR(63) NOT NULL,
    `content`    TEXT        NOT NULL,
    `created_at` DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME    NULL ON UPDATE CURRENT_TIMESTAMP,
    `deleted_at` DATETIME    NULL,

    PRIMARY KEY (`post_id`),

    FOREIGN KEY (`author_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE
);

CREATE INDEX `idx_posts_author_created`
ON `posts` (`author_id`, `created_at`);


-- comments
CREATE TABLE `comments` (
    `comment_id` INT      NOT NULL AUTO_INCREMENT,
    `author_id`  INT      NOT NULL,
    `post_id`    INT      NOT NULL,
    `content`    TEXT     NOT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    `deleted_at` DATETIME NULL,

    PRIMARY KEY (`comment_id`),

    FOREIGN KEY (`author_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE,

    FOREIGN KEY (`post_id`)
        REFERENCES `posts`(`post_id`)
        ON DELETE CASCADE
);

-- Create an index on the comments table to improve query performance for filtering comments by post_id and ordering by created_at.
CREATE INDEX `idx_comments_post_created` ON `comments` (`post_id`, `created_at`);


-- events
CREATE TABLE `events` (
    `event_id`          INT          NOT NULL AUTO_INCREMENT,
    `creator_id`        INT          NOT NULL,
    `title`             VARCHAR(127) NOT NULL,
    `description`       TEXT         NOT NULL,
    `city_id`           INT          NOT NULL,
    `start_time`        DATETIME     NOT NULL,
    `end_time`          DATETIME     NOT NULL,
    `institute_id`      INT          NULL,
    `field_of_study_id` INT          NULL,
    `is_sponsored`      BOOLEAN      NOT NULL DEFAULT FALSE,
    `created_at`        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at`        DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    `deleted_at`        DATETIME     NULL,

    PRIMARY KEY (`event_id`),

    FOREIGN KEY (`creator_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE,

    FOREIGN KEY (`city_id`)
        REFERENCES `cities`(`city_id`),

    FOREIGN KEY (`institute_id`)
        REFERENCES `institutes`(`institute_id`)
        ON DELETE SET NULL,

    FOREIGN KEY (`field_of_study_id`)
        REFERENCES `fields_of_study`(`field_of_study_id`)
        ON DELETE SET NULL,

    CONSTRAINT `chk_event_time`
        CHECK (`end_time` > `start_time`)
);


-- private_messages
CREATE TABLE `private_messages` (
    `message_id`   INT      NOT NULL AUTO_INCREMENT,
    `from_user_id` INT      NOT NULL,
    `to_user_id`   INT      NOT NULL,
    `content`      TEXT     NOT NULL,
    `created_at`   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at`   DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    `deleted_at`   DATETIME NULL,

    PRIMARY KEY (`message_id`),

    FOREIGN KEY (`from_user_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE,

    FOREIGN KEY (`to_user_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE
);


-- Create indexes on the private_messages table to improve query performance for filtering messages by sender and recipient, and ordering by created_at.
CREATE INDEX `idx_messages_sender_sent` ON `private_messages` (`from_user_id`, `created_at`);


-- Create indexes on the private_messages table to improve query performance for filtering messages by recipient and sender, and ordering by created_at.
CREATE INDEX `idx_messages_recipient_sent` ON `private_messages` (`to_user_id`, `created_at`);


-- student_jobs
CREATE TABLE `student_jobs` (
    `student_job_id` INT          NOT NULL AUTO_INCREMENT,
    `partner_id`     INT          NOT NULL,
    `position`       VARCHAR(127) NOT NULL,
    `description`    TEXT         NOT NULL,
    `start_date`     DATE         NOT NULL,
    `deadline`       DATE         NOT NULL,
    `created_at`     DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at`     DATETIME     NULL,
    `deleted_at`     DATETIME     NULL,

    PRIMARY KEY (`student_job_id`),

    FOREIGN KEY (`partner_id`)
        REFERENCES `partners`(`user_id`)
        ON DELETE CASCADE,

    CONSTRAINT `chk_job_deadline`
        CHECK (`deadline` >= `start_date`)
);


-- =========================================================
-- 7. JUNCTION TABLES
-- =========================================================

-- user_follows_user
CREATE TABLE `user_follows_user` (
    `from_user_id` INT      NOT NULL,
    `to_user_id`   INT      NOT NULL,
    `created_at`   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`from_user_id`, `to_user_id`),

    FOREIGN KEY (`from_user_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE,

    FOREIGN KEY (`to_user_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE,

    CONSTRAINT `chk_no_self_follow`
        CHECK (`from_user_id` <> `to_user_id`)
);

-- Create an index on the user_follows_user table to improve query performance for filtering followers by the followed user.
CREATE INDEX `idx_user_follows_user_to` ON `user_follows_user` (`to_user_id`);


-- user_blocks_user
CREATE TABLE `user_blocks_user` (
    `from_user_id` INT      NOT NULL,
    `to_user_id`   INT      NOT NULL,
    `created_at`   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`from_user_id`, `to_user_id`),
    FOREIGN KEY (`from_user_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE,
    FOREIGN KEY (`to_user_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE,

    CONSTRAINT `chk_no_self_block`
        CHECK (`from_user_id` <> `to_user_id`)
);


-- Create an index on the user_blocks_user table to improve query performance for filtering blocked users by the blocking user.
CREATE INDEX `idx_user_blocks_user_to` ON `user_blocks_user` (`to_user_id`);


-- user_interests
CREATE TABLE `user_interests` (
    `user_id`     INT NOT NULL,
    `interest_id` INT NOT NULL,

    PRIMARY KEY (`user_id`, `interest_id`),
    FOREIGN KEY (`user_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE,
    FOREIGN KEY (`interest_id`)
        REFERENCES `interests`(`interest_id`)
        ON DELETE CASCADE
);


-- user_likes_post
CREATE TABLE `user_likes_post` (
    `user_id`    INT      NOT NULL,
    `post_id`    INT      NOT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`user_id`, `post_id`),

    FOREIGN KEY (`user_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE,

    FOREIGN KEY (`post_id`)
        REFERENCES `posts`(`post_id`)
        ON DELETE CASCADE
);


-- user_likes_comment
CREATE TABLE `user_likes_comment` (
    `user_id`    INT      NOT NULL,
    `comment_id` INT      NOT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`user_id`, `comment_id`),

    FOREIGN KEY (`user_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE,

    FOREIGN KEY (`comment_id`)
        REFERENCES `comments`(`comment_id`)
        ON DELETE CASCADE
);


-- post_images
CREATE TABLE `post_images` (
    `post_id`  INT NOT NULL,
    `image_id` INT NOT NULL,

    PRIMARY KEY (`post_id`, `image_id`),

    FOREIGN KEY (`post_id`)
        REFERENCES `posts`(`post_id`)
        ON DELETE CASCADE,

    FOREIGN KEY (`image_id`)
        REFERENCES `images`(`image_id`)
        ON DELETE CASCADE
);


-- event_images
CREATE TABLE `event_images` (
    `event_id` INT NOT NULL,
    `image_id` INT NOT NULL,

    PRIMARY KEY (`event_id`, `image_id`),

    FOREIGN KEY (`event_id`)
        REFERENCES `events`(`event_id`)
        ON DELETE CASCADE,

    FOREIGN KEY (`image_id`)
        REFERENCES `images`(`image_id`)
        ON DELETE CASCADE
);


-- user_attends_event
CREATE TABLE `user_attends_event` (
    `user_id`   INT      NOT NULL,
    `event_id`  INT      NOT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`user_id`, `event_id`),

    FOREIGN KEY (`user_id`)
        REFERENCES `users`(`user_id`)
        ON DELETE CASCADE,

    FOREIGN KEY (`event_id`)
        REFERENCES `events`(`event_id`)
        ON DELETE CASCADE
);
